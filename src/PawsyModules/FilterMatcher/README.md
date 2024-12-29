# Filter Matcher Module

- [Overview](#overview)
- [Rules Configuration](#rules-configuration)
- [Example Filter-Matcher setup](#example-filter-matcher-setup)
- [Information Collection Policy](#information-collection-policy)
    - [Information Deletion Policy](#information-deletion-policy)

## Overview

The filter matcher module allows users with global manage message permissions to configure regex strings that Pawsy will check messages against. Pawsy can do any of the following when a message is found to match the regex:

- Alert in a special alert channel
- Reply to the message with a response
- Delete the message

Configuration:

- `Alert Channel:` The dedicated channel for Pawsy to send alerts

## Rules Configuration

- `Regex:` The actual regex that is matched
- `Delete Message:` Pawsy will delete the matched message
- `Warn Staff:` Pawsy will warn staff in her alert channel
  - `Color:` The color of the embed when warning staff
- `Rule Name:` Will be displayed when warning staff or listing the rule
- `Reply:` The message Pawsy should send as a reply
- `Filter Type:` either Blacklist or Whitelist. Determines where this filter can run
  - `Filtered Channels:` A list of channels for this filter

## Example Filter-Matcher setup

- First, we setup the alert channel by running `/module-config filter-matcher` then selecting the `alert-channel` option and then using the helpful picker to select a channel for Pawsy to alert staff in. This is optional but most of the value of filters comes from alerting staff when they are met so do be sure to set it up
- Next, we add a new basic filter with `/filter-matcher filters add` and fill in the name and regex fields. For testing filters I like to use [Regex101](https://regex101.com/) with the .Net 7.0 (C#) filter type (because Pawsy is C# code) and the /gi flags because Pawsy uses case-insensitive matching and global just lets me test multiple messages at once. For our test filter we'll name it "Test" and use the regex "meow" so Pawsy will match on messages containing the word meow
- Assuming this is your first new rule, it will be rule ID 0. Otherwise you will need to use `/filter-matcher filters list` and get the rule ID for the next section
- Now we edit the rule to actually enable it. By default, the rule is set to whitelist with no channels, so its functionally disabled. Use `/filter-matcher filters edit 0` and then select the optional components you wish to edit and change their values. For our purpose, we'll select the `type` property and change it to blacklist so now Pawsy will run this filter on every channel *except* the ones listed in the `channel` property, which is empty so it will run on every channel. Next, we select the `reply` property and set it to "Meow!" so that Pawsy will reply to every instance of "meow" with her own "Meow!". Finally, we select the `warn-staff` property and set it to false because this is a fun example filter and we don't need to spam our alert channel with it.

## Information Collection Policy

The Filter Matcher Module will store as a raw string any information passed into the `Regex` field of a rule.

### Information Deletion Policy

To remove any retained information simply remove or replace the rule that contains it using `/filter-matcher rule remove [rule-id]`
