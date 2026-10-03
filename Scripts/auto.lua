-- Port of test_1.lua (Logitech G Hub) to Mouse Studio.
--
-- Hold FORWARD: LEFT (fire) is held for you and the cursor is pulled
-- down following the table of the selected gun until FORWARD is released.
--
-- Mouse Studio sets GUN_MODE ("m416" or "beryl") from the "Auto"
-- settings of the profile. It is read on each shot.

GUN_MODE = GUN_MODE or "m416"


local LEFT_BUTTON = 1
local FORWARD_BUTTON = 5

local STEPS = 80
local MOVES_PER_STEP = 4
local STEP_SLEEP = 21


-- The first values of a table, then "hold" repeated up to STEPS entries.
local function buildTable(head, hold)

    local values = {}

    for i = 1, STEPS do

        values[i] = head[i] or hold

    end

    return values

end


local RECOIL = {

    m416 = buildTable(
        { 5, 4, 4, 4, 4, 5, 5, 6, 5, 6, 5, 6, 4, 6, 6, 6, 6, 6, 6 },
        7
    ),

    beryl = buildTable(
        { 6, 5, 5, 5, 6, 7, 7, 8, 7, 8, 8, 8, 7, 9, 9, 8, 8, 8, 8 },
        9
    )
}


-- Nil for an unknown GUN_MODE.
local function getRecoilTable()

    return RECOIL[GUN_MODE]

end


function OnEvent(event, arg)

    if event == "PROFILE_ACTIVATED" then

        OutputLogMessage(
            "Script started (gun=" .. tostring(GUN_MODE) .. ")"
        )

    end


    if event == "MOUSE_BUTTON_PRESSED"
        and arg == FORWARD_BUTTON then

        local recoil = getRecoilTable()

        if recoil == nil then

            OutputLogMessage(
                "Unknown GUN_MODE: " .. tostring(GUN_MODE)
            )

            return

        end

        local step = 1

        PressMouseButton(
            LEFT_BUTTON
        )

        while IsMouseButtonPressed(FORWARD_BUTTON) do

            for _ = 1, MOVES_PER_STEP do

                MoveMouseRelative(
                    0,
                    recoil[step]
                )

                Sleep(
                    STEP_SLEEP
                )

            end


            -- Stay on the last value once the table runs out.
            if step < STEPS then
                step = step + 1
            end

        end


        ReleaseMouseButton(
            LEFT_BUTTON
        )

    end

end
