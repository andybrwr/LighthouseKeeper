import streamDeck from "@elgato/streamdeck";

import { TurnOn } from "./actions/turn-on";
import { TurnOff } from "./actions/turn-off";

// We can enable "trace" logging so that all messages between the Stream Deck, and the plugin are recorded. When storing sensitive information
streamDeck.logger.setLevel("trace");

// Register the increment action.
streamDeck.actions.registerAction(new TurnOn());
streamDeck.actions.registerAction(new TurnOff());

// Finally, connect to the Stream Deck.
streamDeck.connect();
