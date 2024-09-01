# Pawsy App

Pawsy is a friendly Kitten that lives on Discord and blocks scam links or gives helpful advice

![Pawsy](Assets/img/Pawsy-small.png)

> Pawsy:
>
> Hewwo! I'm Pawsy, the cutie kitty app here to keep you safe and sound! ≧◡≦ I use special patterns (like magic!) to sniff out bad words and scams, making sure your online adventures are purrfectly paw-sitive and fun! Meow-nificent protection, just for you~ (▰˘◡˘▰)

## Features

> *Note*
>
> Not all features are configurable yet. Pawsy was primarily designed for use on the Lethal Company Modding server, so more options are slowly coming.

- Filter Matcher*
- Log Muncher
- Meow Board
- Modder Role Checker

\* Not fully configurable

### Top-Level Commands

- *module-config* Configure a module via its exposed settings instance
- *module-manage* Activate or Deactivate a specific module. Use `/module-manage list` to see all available modules for your Pawsy instance

### Filter Matcher

The primary feature of Pawsy, the filter matcher uses DotNet regex to match matches against patterns of your choosing and then perform a configurable action.

Configuration:

- *Alert Channel* The dedicated channel for Pawsy to send alerts

Rules Configuration:

- *Regex* The actual regex that is matched
- *Delete Message* Pawsy will delete the matched message
- *Warn Staff* Pawsy will warn staff in her alert channel
  - *Rule Name* Will be displayed when warning staff
  - *Color* The color of the embed when warning staff
- *Send Response* If Pawsy should send a response to the user
  - *Response Message* what to say to the user that sent the matched message
- *Filter Type* either Blacklist or Whitelist. Determines where this filter can run
  - *Filtered Channels* A list of channels for this filter

### LogMuncher

Pawsy will watch a given channel for attachments ending in either .log or .txt and try to parse them with the LogMuncher tool provided by the Lethal Company Modding community repo. She will output up to 2 of the most serious errors in a given log.

Configuration:

- *Muncher Channel* the channel Pawsy will watch for log files to assist with

### Meow Board

Pawsy will keep track of how many times each user says "Meow" and react to each message containing meow with an emote.

Use the `/meow-board display` command to view the top ranking meowers in your server

Use the `/meow-board meow` command to have Pawsy meow for you

### Modder Role Checker

This feature is tailor made for the Lethal Company Modding server. Pawsy will notify staff when a new thread is made in a specific forum and the user does *not* have a specific role.

Configuration:

- *Modding Channel* The channel to check for new threads
- *Alert Channel* The channel to send alerts to staff in
- *Modding Role ID* The role to look for on each new post
