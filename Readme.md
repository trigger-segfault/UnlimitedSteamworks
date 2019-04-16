# Unlimited Steamworks Launcher ![AppIcon](https://i.imgur.com/lxLaZ7X.png)

[![Latest Release](https://img.shields.io/github/release-pre/trigger-death/UnlimitedSteamworksLauncher.svg?style=flat&label=version)](https://github.com/trigger-death/UnlimitedSteamworksLauncher/releases/latest)
[![Latest Release Date](https://img.shields.io/github/release-date-pre/trigger-death/UnlimitedSteamworksLauncher.svg?style=flat&label=released)](https://github.com/trigger-death/UnlimitedSteamworksLauncher/releases/latest)
[![Total Downloads](https://img.shields.io/github/downloads/trigger-death/UnlimitedSteamworksLauncher/total.svg?style=flat)](https://github.com/trigger-death/UnlimitedSteamworksLauncher/releases)
[![Creation Date](https://img.shields.io/badge/created-april%202019-A642FF.svg?style=flat)](https://github.com/trigger-death/UnlimitedSteamworksLauncher/commit/b6691de97410dac4648ca32303415940113e02f8)
[![Discord](https://img.shields.io/discord/436949335947870238.svg?style=flat&logo=discord&label=chat&colorB=7389DC&link=https://discord.gg/vB7jUbY)](https://discord.gg/vB7jUbY)

A launcher for Steam apps to signal Steam activity, protect against crashing apps breaking relaunching, and circumventing Steam's terrible command line argument security measures.

Unlimited Steamworks is for launching Steam games **without** the use of the Steam client. All issues aside from the 2nd point are moot when a game is launched via the Steam client.

This launcher may have unintended side effects if the game being run has built in Steam activity support.

***

<p align="center"><i>
I am the leather of my wallet.<br/>
Deals are my Pursuit and Unplayed Games are my Library.<br/>
I have wasted over a Thousand Hours,<br/>
Unsold on the Market,<br/>
Nor used in a Booster Pack.<br/>
I have swallowed my Dignity to leave Shitpost Curations,<br/>
Yet these Cards will never form a Badge.<br/>
So, as I browse the sale--<br/>
<b>UNLIMITED STEAMWORKS!</b><br/>
</i></p>

***

## Usage

In order to start a game with Steam activity, you need to know the Steam App ID of the game. The App ID for a game, can be found at the end of the URL for the game's store page, or by searching the game on [steamdb](https://steamdb.info/).

```
Usage: UnlimitedSteamworks.exe -a <appid> -r <path> [-e] [-p name] [-w dir] [-d sec] [-i sec] [-- args...]

Options:
   -a, --app  APPID           ID of the Steam App to launch.
   -r, --run  PATH            Path to the program to run.
   -e, --elevate              Run the program as an administrator.
   -p, --proc  NAME           Name of the process to detect. (without extension)
   -w, --working  DIRECTORY   Working directory to start in.
   -d, --delay  SECONDS       Delay before process detection starts. (DEFAULT=1.0)
   -i, --interval  SECONDS    Process detection interval. (DEFAULT=1.0)
   -h, --help                 Show this help message.
```

`UnlimitedSteamworksProc.exe` will run in the background to keep your Steam activity status alive until it no longer detects the process name. Specify a custom process name if the program being run is a launcher for a different program.

If the process name is passed as an empty string, the process will not be detected, and Steam activity will be maintained by the launched program. This does not work if you are running as an administrator.

## Creating Shortcuts

To create a shortcut for a game: Copy `UnlimitedSteamworks.exe` then right click in the desired location of the shortcut and select Paste Shortcut. From there, open the shortcut's properties and add your command line arguments to the end of the Target field.

By changing the Start In field, the Launcher will look for a `steamworks_args.json` file and or a `steam_appid.txt`. If `steam_appid.txt` is found and the App ID was not passed in the arguments, then the contents of this file will be used as the appid. Many Steam games will already have this file within their installation directory. If `steamworks_args.json` is found and no arguments were passed, then the contents of this file will be used as the arguments for the launcher. The name of the argument fields in the json file is the same as the name of the option in the command line. The launcher will use the App ID found in `steamworks_args.json` over `steam_appid.txt` when both are present and the app field is defined in the json file.

**Note:** The location of the shortcut itself has no effect.

This same behavior can be mimicked with batch files, or any other tool that can launch another program.

## Configuration Settings

The `steamworks_config.json` file created in the Unlimited Steamworks program directory after running the program once can be modified to change to change a small selection of settings, mostly revolving around when the program should display errors or error out.

|Field|Description|
|:--|:--|
|`cleanup`|Disables cleanup of the `steamworks_args.json` and `steam_appid.txt` generated in the Unlimited Steamworks program directory.
|`proc_exe`|The name of the `UnlimitedSteamworksProc.exe` which is used to launch the program and assign Steam activity. If you want to rename the program, then change this field. It must stay in the same directory as `UnlimitedSteamworks.exe`.
|`silent`|The program will not display a console when an error occurs and will only log errors to `steamworks_errors.log`. The only time silent is ignored is when the help option is passed to `UnlimitedSteamworks.exe`.
|`error_steam_activity`|The program will throw an error if it fails to initiate Steam activity. This is true by default.|
|`error_steam_not_running`|The program will throw an error if the Steam client is not running. This is false by default|

### Custom Configurations

Adding a `config` field to `steamworks_args.json` in the working directory will allow you to change existing configuration options when launching an a game. The only configuration field that is ignored is `proc_exe`.

## Examples

Here is an example usage of `UnlimitedSteamworks.exe`. The `--` with no option attached is used to signal the beginning of command line arguments that are passed to the game being launched.

```ps
UnlimitedSteamworks.exe -a 123456 -r "C:\Steam\steamapps\common\MyGame\MyGameLauncher.exe" -p MyGame -d 2.0 -- --trace on
```

Here is an example `steamworks_args.json` file with the optional `config` field specified. Any field that is null or not specified in the json uses the default value if it has one.

```json
{
  "app": 123456,
  "run": "C:\\Steam\\steamapps\\common\\MyGame\\MyGameLauncher.exe",
  "elevate": false,
  "proc": "MyGame",
  "working": null,
  "delay": 2.0,
  "interval": 1.0,
  "args": [
    "--trace",
    "on"
  ],
  "config": {
    "cleanup": true,
    "silent": true,
    "error_steam_activity": true,
    "error_steam_not_running": false
  }
}
```
