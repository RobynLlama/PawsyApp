# Pawsy App

![Pawsy-banner](Assets/img/Pawsy-banner.png)
> Pawsy:
>
> Hewwo! I'm Pawsy, the cutie kitty app here to keep you safe and sound! ≧◡≦ I use special patterns (like magic!) to sniff out bad words and scams, making sure your online adventures are purrfectly paw-sitive and fun! Meow-nificent protection, just for you~ (▰˘◡˘▰)

Pawsy is a friendly Kitten that lives on Discord and blocks scam links or gives helpful advice

- [Features](#features)
  - [Top-Level Commands](#top-level-commands)
  - [Filter Matcher](#filter-matcher)
    - [Example Filter-Matcher setup](#example-filter-matcher-setup)
  - [LogMuncher](#logmuncher)
  - [Meow Board](#meow-board)
  - [Forum Role Checker](#forum-role-checker)
    - [Commands](#commands)

## Features

- Filter Matcher
- Log Muncher
- Meow Board
- Forum Role Checker

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

#### Example Filter-Matcher setup

- First, we setup the alert channel by running `/module-config filter-matcher` then selecting the `alert-channel` option and then using the helpful picker to select a channel for Pawsy to alert staff in. This is optional but most of the value of filters comes from alerting staff when they are met so do be sure to set it up
- Next, we add a new basic filter with `/filter-matcher filters add` and fill in the name and regex fields. For testing filters I like to use [Regex101](https://regex101.com/) with the .Net 7.0 (C#) filter type (because Pawsy is C# code) and the /gi flags because Pawsy uses case-insensitive matching and global just lets me test multiple messages at once. For our test filter we'll name it "Test" and use the regex "meow" so Pawsy will match on messages containing the word meow
- Assuming this is your first new rule, it will be rule ID 0. Otherwise you will need to use `/filter-matcher filters list` and get the rule ID for the next section
- Now we edit the rule to actually enable it. By default, the rule is set to whitelist with no channels, so its functionally disabled. Use `/filter-matcher filters edit 0` and then select the optional components you wish to edit and change their values. For our purpose, we'll select the `type` property and change it to blacklist so now Pawsy will run this filter on every channel *except* the ones listed in the `channel` property, which is empty so it will run on every channel. Next, we select the `reply` property and set it to "Meow!" so that Pawsy will reply to every instance of "meow" with her own "Meow!". Finally, we select the `warn-staff` property and set it to false because this is a fun example filter and we don't need to spam our alert channel with it.

This concludes the absolute basics of how to use Pawsy's filter module.

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
