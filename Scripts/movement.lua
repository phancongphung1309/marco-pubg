MOVE_X = MOVE_X or 0
MOVE_Y = MOVE_Y or 5
INTERVAL = INTERVAL or 10

ACTIVE_DURATION =
    ACTIVE_DURATION or 5000

REPEAT_DELAY =
    REPEAT_DELAY or 1000


local LEFT_BUTTON = 1


function OnEvent(event, arg)

    if event == "PROFILE_ACTIVATED" then

        OutputLogMessage(
            "Script started"
        )

    end


    -- Sent when the hotkey assigned in the app is pressed
    -- (Forward side button by default).
    if event == "HOTKEY_PRESSED" then

        OutputLogMessage(
            "Hotkey pressed"
        )


        while IsHotkeyPressed() do

            -- =================================
            -- ACTIVE PHASE
            -- =================================

            local startTime =
                GetTickCount()

            PressMouseButton(
                LEFT_BUTTON
            )

            OutputLogMessage(
                "Activated"
            )


            while IsHotkeyPressed() do

                local elapsed =
                    GetTickCount()
                    - startTime


                if elapsed >= ACTIVE_DURATION then
                    break
                end


                MoveMouseRelative(
                    MOVE_X,
                    MOVE_Y
                )

                Sleep(
                    INTERVAL
                )

            end


            ReleaseMouseButton(
                LEFT_BUTTON
            )


            -- Nếu đã thả phím nóng
            if not IsHotkeyPressed() then

                break

            end


            OutputLogMessage(
                "Cooldown"
            )


            -- =================================
            -- COOLDOWN / REPEAT DELAY
            -- =================================

            local cooldownStart =
                GetTickCount()


            while IsHotkeyPressed() do

                local cooldownElapsed =
                    GetTickCount()
                    - cooldownStart


                if cooldownElapsed >= REPEAT_DELAY then
                    break
                end


                Sleep(10)

            end


            -- Nếu thả phím nóng trong cooldown
            if not IsHotkeyPressed() then

                break

            end


            OutputLogMessage(
                "Reactivated"
            )

        end


        ReleaseMouseButton(
            LEFT_BUTTON
        )


        OutputLogMessage(
            "Hotkey released"
        )

    end

end