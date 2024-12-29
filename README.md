# Pawsy App

![Pawsy-banner](Assets/img/Pawsy-banner.png)
> Pawsy:
>
> Hewwo! I'm Pawsy, the cutie kitty app here to keep you safe and sound! ≧◡≦ I use special patterns (like magic!) to sniff out bad words and scams, making sure your online adventures are purrfectly paw-sitive and fun! Meow-nificent protection, just for you~ (▰˘◡˘▰)

Pawsy is a friendly Kitten that lives on Discord and blocks scam links or gives helpful advice

- [Modules](#modules)
  - [Top-Level Commands](#top-level-commands)
  - [LogMuncher](#logmuncher)
  - [Meow Board](#meow-board)
  - [Forum Role Checker](#forum-role-checker)
    - [Commands](#commands)

## Modules

> Note that each instance of Pawsy may have its own selection of modules chosen by its operator and what, if any, information collected may be different. Please familiarize yourself with the collection policy of any modules you choose to enable in your server before activating them.

- Filter Matcher
- Log Muncher
- Meow Board
- Forum Role Checker

### Top-Level Commands

- *module-config* Configure a module via its exposed settings instance
- *module-manage* Activate or Deactivate a specific module. Use `/module-manage list` to see all available modules for your Pawsy instance

### LogMuncher

Pawsy will watch a given channel for attachments ending in either .log or .txt and try to parse them with the LogMuncher tool provided by the Lethal Company Modding community repo. She will output up to 2 of the most serious errors in a given log.

Configuration:

- *Muncher Channel* the channel Pawsy will watch for log files to assist with

### Meow Board

Pawsy will peridiocally send a treasure hunt message to the specified channel and anyone that claims gains meows on the Meow Board.

Use the `/meow-board display` command to view the top ranking meowers in your server

Use the `/meow-board meow` command to have Pawsy meow for you

Configuration:

- *Game Channel* the channel Pawsy will send treasure hunts to

### Forum Role Checker

This feature is tailor made for the Lethal Company Modding server. Pawsy will notify staff when a new thread is made in a specific forum and the user does *not* have a specific role.

Configuration:

- *Alert Channel* The channel to send alerts to staff in

#### Commands

- `forum-role-checker` `add-watch-channel`
  - Used to add a specific forum channel and role combo to the watch list. Pawsy will alert you when somebody posts in this forum and doesn't have the role you select. Uses handy drop-down selectors to make setting up really easy!
- `forum-role-checker` `remove-watch-channel`
  - Removes a channel from the watch list. Make absolutely sure you do not delete channels that Pawsy is watching or they will be stuck on the list since this uses a handy drop-down selector that only shows existing channels!
