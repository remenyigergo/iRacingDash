1) Install

1.1) Download the .rar file (iRacingDash_v#.#.#.#.rar)
1.2) Unrar the package to a folder


2) Usage

2.1) It is required to set up the "externalConfig.config" file.
	This file is an XML file. The config data that is only information that it should contain.
	You shouldn't change anything beside the values. It will most likely break the application.
	The first line you mustn't change either. 

	These data are initial values to the application.

2.1.1) Car
	MaxRpm - the actual car's maximum rpm

2.1.2) Window
	PositionX - X coordinate of the window
	PositionY - Y coordinate of the window

2.1.3) Leds
	The leds working in 3 portions.
		1st: the green leds
		2nd: yellow leds
		3rd: red leds
	These are lighted in percentage distribution. So the three should add up to 100% percent.

	MinimumRPMPercent - this is the baseline value when the green leds should start to light up.
	ShiftLightGreenPercent - this is the percentage the green leds will last. So it adds to the MinimumRPMPercent.  E.g: If minimumRPMPercent = 30%, and ShiftLightGreenPercent = 25% then the green leds will light in the 30%-55% range.
	ShiftLightYellowPercent - same as before
	ShiftLightRedPercent - same as before

2.1.4) Log
	First, put the externalConfig files next to the .exe files. (WECOverlay and iRacingDash)
	Logging values to folders if any error or miscalculation were made.

	Path - path to any folder. Default for me is C:\User\<user>\Documents\iRacingDash\logs

2.1.5) Fps
	TelemetryFps - basically the data's refresh time in one second (usually 60fps is way too much and using CPU much more. It can be used, but not recommended. Stronger PCs are in favor here)
	NonRealtimeCalculationsFps - there are some values that doesn't need to be refreshed for 30 times in a second because of the less important role of the data like fuel remaining. Most of the time it's not that relevant to calculate everything fuel-related data in such a speed. This is an optimization for weaker CPU-s.
	
3) Run the iRacingDash.exe