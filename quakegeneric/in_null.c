/*
Copyright (C) 1996-1997 Id Software, Inc.

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  

See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

*/
// in_null.c -- for systems without a mouse

#include "quakedef.h"

#include "quakegeneric.h"

#define JOY_ABSOLUTE_AXIS	0x00000000		// control like a joystick
#define JOY_RELATIVE_AXIS	0x00000010		// control like a mouse, spinner, trackball

enum _ControlList
{
	AxisNada = 0, AxisForward, AxisLook, AxisSide, AxisTurn
};

float	mouse_x, mouse_y;
cvar_t	in_joystick = {"joystick","0", true};
cvar_t	joy_advanced = {"joyadvanced", "0"};
cvar_t	joy_advaxisx = {"joyadvaxisx", "0"};
cvar_t	joy_advaxisy = {"joyadvaxisy", "0"};
cvar_t	joy_advaxisz = {"joyadvaxisz", "0"};
cvar_t	joy_advaxisr = {"joyadvaxisr", "0"};
cvar_t	joy_advaxisu = {"joyadvaxisu", "0"};
cvar_t	joy_advaxisv = {"joyadvaxisv", "0"};
cvar_t	joy_forwardthreshold = {"joyforwardthreshold", "0.15"};
cvar_t	joy_sidethreshold = {"joysidethreshold", "0.15"};
cvar_t	joy_pitchthreshold = {"joypitchthreshold", "0.15"};
cvar_t	joy_yawthreshold = {"joyyawthreshold", "0.15"};
cvar_t	joy_forwardsensitivity = {"joyforwardsensitivity", "-1.0"};
cvar_t	joy_sidesensitivity = {"joysidesensitivity", "-1.0"};
cvar_t	joy_pitchsensitivity = {"joypitchsensitivity", "1.0"};
cvar_t	joy_yawsensitivity = {"joyyawsensitivity", "-1.0"};

qboolean	joy_advancedinit;
int dwAxisMap[QUAKEGENERIC_JOY_MAX_AXES];
int dwControlMap[QUAKEGENERIC_JOY_MAX_AXES];

void Joy_AdvancedUpdate_f (void);


void IN_Init (void)
{
	Cvar_RegisterVariable (&in_joystick);
	Cvar_RegisterVariable (&joy_advanced);
	Cvar_RegisterVariable (&joy_advaxisx);
	Cvar_RegisterVariable (&joy_advaxisy);
	Cvar_RegisterVariable (&joy_advaxisz);
	Cvar_RegisterVariable (&joy_advaxisr);
	Cvar_RegisterVariable (&joy_advaxisu);
	Cvar_RegisterVariable (&joy_advaxisv);
	Cvar_RegisterVariable (&joy_forwardthreshold);
	Cvar_RegisterVariable (&joy_sidethreshold);
	Cvar_RegisterVariable (&joy_pitchthreshold);
	Cvar_RegisterVariable (&joy_yawthreshold);
	Cvar_RegisterVariable (&joy_forwardsensitivity);
	Cvar_RegisterVariable (&joy_sidesensitivity);
	Cvar_RegisterVariable (&joy_pitchsensitivity);
	Cvar_RegisterVariable (&joy_yawsensitivity);

	Cmd_AddCommand ("joyadvancedupdate", Joy_AdvancedUpdate_f);

	joy_advancedinit = false;
}

void IN_Shutdown (void)
{
}

void IN_Commands (void)
{
	int down;
	int key;

	while (QG_GetKey(&down, &key)) {
		Key_Event(key, down);
	}
}

void IN_MouseMove (usercmd_t *cmd)
{
	int		mx, my;

	QG_GetMouseMove(&mx, &my);

	mouse_x = mx;
	mouse_y = my;

	mouse_x *= sensitivity.value;
	mouse_y *= sensitivity.value;

	// add mouse X/Y movement to cmd
	if ( (in_strafe.state & 1) || (lookstrafe.value && (in_mlook.state & 1) ))
		cmd->sidemove += m_side.value * mouse_x;
	else
		cl.viewangles[YAW] -= m_yaw.value * mouse_x;

	if (in_mlook.state & 1)
		V_StopPitchDrift ();

	if ( (in_mlook.state & 1) && !(in_strafe.state & 1))
	{
		cl.viewangles[PITCH] += m_pitch.value * mouse_y;
		if (cl.viewangles[PITCH] > 80)
			cl.viewangles[PITCH] = 80;
		if (cl.viewangles[PITCH] < -70)
			cl.viewangles[PITCH] = -70;
	}
	else
	{
		if ((in_strafe.state & 1) && noclip_anglehack)
			cmd->upmove -= m_forward.value * mouse_y;
		else
			cmd->forwardmove -= m_forward.value * mouse_y;
	}
}

void Joy_AdvancedUpdate_f (void)
{

	// called once by IN_ReadJoystick and by user whenever an update is needed
	// cvars are now available
	int	i;
	int dwTemp;

	// initialize all the maps
	for (i = 0; i < QUAKEGENERIC_JOY_MAX_AXES; i++)
	{
		dwAxisMap[i] = AxisNada;
		dwControlMap[i] = JOY_ABSOLUTE_AXIS;
	}

	if( joy_advanced.value == 0.0)
	{
		// default joystick initialization
		// 2 axes only with joystick control
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_X] = AxisTurn;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_Y] = AxisForward;
	}
	else
	{
		// advanced initialization here
		// data supplied by user via joy_axisn cvars
		dwTemp = (int) joy_advaxisx.value;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_X] = dwTemp & 0x0000000f;
		dwControlMap[QUAKEGENERIC_JOY_AXIS_X] = dwTemp & JOY_RELATIVE_AXIS;
		dwTemp = (int) joy_advaxisy.value;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_Y] = dwTemp & 0x0000000f;
		dwControlMap[QUAKEGENERIC_JOY_AXIS_Y] = dwTemp & JOY_RELATIVE_AXIS;
		dwTemp = (int) joy_advaxisz.value;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_Z] = dwTemp & 0x0000000f;
		dwControlMap[QUAKEGENERIC_JOY_AXIS_Z] = dwTemp & JOY_RELATIVE_AXIS;
		dwTemp = (int) joy_advaxisr.value;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_R] = dwTemp & 0x0000000f;
		dwControlMap[QUAKEGENERIC_JOY_AXIS_R] = dwTemp & JOY_RELATIVE_AXIS;
		dwTemp = (int) joy_advaxisu.value;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_U] = dwTemp & 0x0000000f;
		dwControlMap[QUAKEGENERIC_JOY_AXIS_U] = dwTemp & JOY_RELATIVE_AXIS;
		dwTemp = (int) joy_advaxisv.value;
		dwAxisMap[QUAKEGENERIC_JOY_AXIS_V] = dwTemp & 0x0000000f;
		dwControlMap[QUAKEGENERIC_JOY_AXIS_V] = dwTemp & JOY_RELATIVE_AXIS;
	}
}

// Adapted from Quake's in_win.c, since in_dos.c only supports discrete joystick inputs.
void IN_JoyMove (usercmd_t *cmd)
{
	float	axisValues[QUAKEGENERIC_JOY_MAX_AXES] = {0};
	float	speed, aspeed;
	float	fAxisValue, fTemp;
	int		i;

	if ( joy_advancedinit != true )
	{
		Joy_AdvancedUpdate_f();
		joy_advancedinit = true;
	}

	if (!in_joystick.value)
		return;

	// collect the joystick data, if possible
	QG_GetJoyAxes(axisValues);

	if (in_speed.state & 1)
		speed = cl_movespeedkey.value;
	else
		speed = 1;
	aspeed = speed * host_frametime;

	// loop through the axes
	for (i = 0; i < QUAKEGENERIC_JOY_MAX_AXES; i++)
	{
		fAxisValue = axisValues[i];

		switch (dwAxisMap[i])
		{
		case AxisForward:
			if ((joy_advanced.value == 0.0) && (in_mlook.state & 1))
			{
				// user wants forward control to become look control
				if (fabs(fAxisValue) > joy_pitchthreshold.value)
				{
					// if mouse invert is on, invert the joystick pitch value
					// only absolute control support here (joy_advanced is false)
					if (m_pitch.value < 0.0)
					{
						cl.viewangles[PITCH] -= (fAxisValue * joy_pitchsensitivity.value) * aspeed * cl_pitchspeed.value;
					}
					else
					{
						cl.viewangles[PITCH] += (fAxisValue * joy_pitchsensitivity.value) * aspeed * cl_pitchspeed.value;
					}
					V_StopPitchDrift();
				}
				else
				{
					// no pitch movement
					// disable pitch return-to-center unless requested by user
					// *** this code can be removed when the lookspring bug is fixed
					// *** the bug always has the lookspring feature on
					if(lookspring.value == 0.0)
						V_StopPitchDrift();
				}
			}
			else
			{
				// user wants forward control to be forward control
				if (fabs(fAxisValue) > joy_forwardthreshold.value)
				{
					cmd->forwardmove += (fAxisValue * joy_forwardsensitivity.value) * speed * cl_forwardspeed.value;
				}
			}
			break;

		case AxisSide:
			if (fabs(fAxisValue) > joy_sidethreshold.value)
			{
				cmd->sidemove += (fAxisValue * joy_sidesensitivity.value) * speed * cl_sidespeed.value;
			}
			break;

		case AxisTurn:
			if ((in_strafe.state & 1) || (lookstrafe.value && (in_mlook.state & 1)))
			{
				// user wants turn control to become side control
				if (fabs(fAxisValue) > joy_sidethreshold.value)
				{
					cmd->sidemove -= (fAxisValue * joy_sidesensitivity.value) * speed * cl_sidespeed.value;
				}
			}
			else
			{
				// user wants turn control to be turn control
				if (fabs(fAxisValue) > joy_yawthreshold.value)
				{
					if(dwControlMap[i] == JOY_ABSOLUTE_AXIS)
					{
						cl.viewangles[YAW] += (fAxisValue * joy_yawsensitivity.value) * aspeed * cl_yawspeed.value;
					}
					else
					{
						cl.viewangles[YAW] += (fAxisValue * joy_yawsensitivity.value) * speed * 180.0;
					}

				}
			}
			break;

		case AxisLook:
			if (in_mlook.state & 1)
			{
				if (fabs(fAxisValue) > joy_pitchthreshold.value)
				{
					// pitch movement detected and pitch movement desired by user
					if(dwControlMap[i] == JOY_ABSOLUTE_AXIS)
					{
						cl.viewangles[PITCH] += (fAxisValue * joy_pitchsensitivity.value) * aspeed * cl_pitchspeed.value;
					}
					else
					{
						cl.viewangles[PITCH] += (fAxisValue * joy_pitchsensitivity.value) * speed * 180.0;
					}
					V_StopPitchDrift();
				}
				else
				{
					// no pitch movement
					// disable pitch return-to-center unless requested by user
					// *** this code can be removed when the lookspring bug is fixed
					// *** the bug always has the lookspring feature on
					if(lookspring.value == 0.0)
						V_StopPitchDrift();
				}
			}
			break;

		default:
			break;
		}
	}

	// bounds check pitch
	if (cl.viewangles[PITCH] > 80.0)
		cl.viewangles[PITCH] = 80.0;
	if (cl.viewangles[PITCH] < -70.0)
		cl.viewangles[PITCH] = -70.0;

}

void IN_Move (usercmd_t *cmd)
{
	IN_MouseMove (cmd);
	IN_JoyMove (cmd);
}

