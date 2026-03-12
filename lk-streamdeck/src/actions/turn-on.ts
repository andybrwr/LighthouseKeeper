import { action, KeyDownEvent, SingletonAction, WillAppearEvent } from "@elgato/streamdeck";

type TurnOnSettings = {};

@action({ UUID: "dev.brwr.lighthousekeeper.turnon" })
export class TurnOn extends SingletonAction<TurnOnSettings> {
    override onWillAppear(ev: WillAppearEvent<TurnOnSettings>): Promise<void> | void {
        return ev.action.setTitle("Turn On");
    }

    override async onKeyDown(ev: KeyDownEvent<TurnOnSettings>): Promise<void> {
        await fetch("http://localhost:12367/on")
    }
}
