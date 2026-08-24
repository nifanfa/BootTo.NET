public static unsafe class UefiHelpers
{
    /// <summary>
    /// Disconnects every driver from a controller while leaving its parent
    /// controller available for reconnecting.
    /// </summary>
    public static EFI_STATUS DisconnectController(EFI_HANDLE controller)
    {
        return gBS->DisconnectController(controller, null, null);
    }

    /// <summary>
    /// Disconnects every controller handle that exposes the specified protocol.
    /// </summary>
    /// <remarks>
    /// EFI_NOT_FOUND means that no matching handles exist and is treated as success.
    /// All matching handles are attempted; the first disconnect error is returned.
    /// </remarks>
    public static EFI_STATUS DisconnectControllers(EFI_GUID protocolGuid)
    {
        EFI_HANDLE* handles = null;
        ulong handleCount = 0;
        EFI_STATUS status = gBS->LocateHandleBuffer(
            ByProtocol,
            &protocolGuid,
            null,
            &handleCount,
            &handles);

        if ((ulong)status == EFI_NOT_FOUND)
            return EFI_SUCCESS;
        if ((ulong)status != EFI_SUCCESS)
            return status;

        EFI_STATUS result = EFI_SUCCESS;
        for (ulong i = 0; i < handleCount; i++)
        {
            EFI_STATUS disconnect = DisconnectControllersOnHandle(handles[i], &protocolGuid);
            if ((ulong)disconnect != EFI_SUCCESS && (ulong)result == EFI_SUCCESS)
                result = disconnect;
        }

        if (handles != null)
            gBS->FreePool(handles);

        return result;
    }

    private static EFI_STATUS DisconnectControllersOnHandle(EFI_HANDLE controller, EFI_GUID* protocolGuid)
    {
        EFI_OPEN_PROTOCOL_INFORMATION_ENTRY* protocolInfo = null;
        ulong protocolInfoCount = 0;
        EFI_STATUS status = gBS->OpenProtocolInformation(
            controller,
            protocolGuid,
            &protocolInfo,
            &protocolInfoCount);

        if ((ulong)status == EFI_NOT_FOUND)
            return EFI_SUCCESS;
        if ((ulong)status != EFI_SUCCESS)
            return status;

        EFI_STATUS result = EFI_SUCCESS;
        for (ulong i = 0; i < protocolInfoCount; i++)
        {
            if ((protocolInfo[i].Attributes & EFI_OPEN_PROTOCOL_BY_DRIVER) == 0)
                continue;

            EFI_STATUS disconnect = gBS->DisconnectController(
                controller,
                protocolInfo[i].AgentHandle,
                null);
            if ((ulong)disconnect != EFI_SUCCESS && (ulong)result == EFI_SUCCESS)
                result = disconnect;
        }

        if (protocolInfo != null)
            gBS->FreePool(protocolInfo);

        return result;
    }
}
