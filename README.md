# T4C_Translator
A windows application that monitors and automatically translates the log file from The 4th Coming (T4C) from source language to output language and allows players to communicate via translation in the app across language barriers. For example, French to English so that players who do not know French can play and interact with NPC's or Players on French servers. This program in real-time will translate all text in the log, as well as allow for manual translation that is copied to the clipboard for interacting with players in different native languages, copy keywords, etc.
## Authors

- [@ccorrin1985](https://www.github.com/ccorrin1985)

## Latest Version Download

 - [Release v2.0](https://drive.google.com/file/d/1LBF9KbfMr_tWf0CFLog-C0t9ad_SpIVr/view?usp=sharing)

## Old Version History

 - [Release v1.6](https://drive.google.com/file/d/1gExa4_oWznHJ3EU2HF8DOWxXY2t6E1qt/view?usp=sharing)
 - [Release v1.5](https://drive.google.com/file/d/1vtGSp9C04m1A12bLotSvJvgxQiiH6ePk/view?usp=sharing)
 - [Release v1.4](https://drive.google.com/file/d/1FcvypXZw_oeNmFTJC8dzSDtTzbOEZMnc/view?usp=sharing)
 - [Release v1.3](https://drive.google.com/file/d/18bNbIz8ZduJjJ7GhQlNDf4AuaA2Ro-oE/view?usp=sharing)
 - [Release v1.2](https://drive.google.com/file/d/10ePqKKeGBmEwBatiK29_avOoZ5KKTdy4/view?usp=sharing)
 - [Release v1.1](https://drive.google.com/file/d/11o-op9-Z7VRCZ7JoqNrQtnfnkDcKRHqT/view?usp=sharing)
 - [Release v1.0](https://drive.google.com/file/d/105F4CheSc6ZXg9qmdmZubzTyOSwVjW37/view?usp=sharing)
 - [Beta v0.2](https://drive.google.com/file/d/1dz24p0s8e9165BwhOih-2rVQ1WUhwt-o/view?usp=sharing)
 - [Beta v0.1](https://drive.google.com/file/d/1-_xlp8n02mE-lPHURtT0YwhS0Zk_On07/view?usp=drive_link)


## Screenshots

 - [Latest Screenshot - Release-v2.0](https://drive.google.com/file/d/1k84Ht0yPw2FNpJOIxXJrUAhrL--0Y-7l/view?usp=sharing)
![Alt text](/Media/release-screenshot-v2.0.png?raw=true "Latest Release")

 - [New Features Overview - Release-v2.0](https://drive.google.com/file/d/1iW4TA8Yn-UqAWjQXB-NSNWWi6HdssVtp/view?usp=sharing)
![Alt text](/Media/new-features-screenshot-v2.0.png?raw=true "New Features")

## Release Notes

 #### Release v2.0
 Major release, full local translation support using optional libre translate docker image, program can auto install the image if not found and auto start it, complete refactoring of the codebase for performance, API limit feature to stay within free API tier, ability to hide non-translated text, clicking translated keywords now copies in original language, better color formatting for keywords and words of interest, fixed bugs with different date/time formats in windows, better log output, better support for resolutions and re-sizeable UI enhancements.
 #### Release v1.6
 Major release, keywords are now in bold white and clickable, which allows for copy/paste into the game client. The new feature allows for quick translation and copying to the clipboard frequently used words. The new feature allows for adding chat channels to an ignore list, and saving on translation calls by toggling the ignore feature. Keywords now show in white; words of interest now show in gray. Enhanced UI experience.
 #### Release v1.5
 Minor release, complete code-refactor, keywords in client now bold in the output log, each new line timestamp now bold in the output log, support for all resolutions via dynamic scaling of the UI, text colors available now match the game client text colors, fixed some bugs with text being sent to the translation API that did not need to be translated, added rich text support and different shading for original and translated text to the output log for better readability
 #### Release v1.4
 Major release, only calling the translation API when a full translation is needed. Now ignore system messages, text that is already in the target language and only sends text to the API that needs translation. Previously all text was sent to the API even if it was already in the target language (English client allows for base NPC dialog in English only custom content is not in the client language as of this release)
 #### Release v1.3
 Minor UI enhancements and cleaned up some form controls.
 #### Release v1.2 
 Major feature addition, log file shows latest log on top, manual translation options, with auto copy to clipboard of translated text.
 #### Release v1.1 
 Minor bug fixes to the translation API and log file
 #### Release v1.0 
 Major release, new UI that scaled with window resizing.
 ####  Beta v0.2 
 Minor bug fixes to the translation API and log file
 #### Beta v0.1 
 Rough first beta release, log file translation only


## Acknowledgements

 - [Awesome Readme Templates](https://awesomeopensource.com/project/elangosundar/awesome-README-templates)
 - [Awesome README](https://github.com/matiassingers/awesome-readme)
 - [How to write a good readme](https://bulldogjob.com/news/449-how-to-write-a-good-readme-for-your-github-project)

## Mandatory Preparation

.NET Desktop Runtime (Latest). The .NET Desktop Runtime enables you to run existing Windows desktop applications. This release includes the .NET Runtime, you don't need to install it separately. You can find it on the right side of the page, second item below.

 - [.NET Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
	
## Optional Preparation (for using the local translation feature)

CPU Virtualization (Enabled in bios). Virtualization features for your CPU must be enabled in your computer's BIOS. This is required for Docker to work. You can find instructions on how to enable virtualization in your BIOS by searching for your computer's model and "enable virtualization". This is different depending on your CPU and motherboard.

 - [CPU Virtualization](https://www.google.com/search?q=enable+virtualization+on+my+cpu+and+motherboard&sca_esv=24731a508b288d28&sca_upv=1&ei=x0HbZrLfENvJwN4P0aqhyQs&ved=0ahUKEwjy3svD8a6IAxXbJNAFHVFVKLkQ4dUDCBA&uact=5&oq=enable+virtualization+on+my+cpu+and+motherboard&gs_lp=Egxnd3Mtd2l6LXNlcnAiL2VuYWJsZSB2aXJ0dWFsaXphdGlvbiBvbiBteSBjcHUgYW5kIG1vdGhlcmJvYXJkMgoQIRigARjDBBgKSKsnUMYUWJgmcAJ4AZABAJgBjAGgAb8JqgEEMC4xMLgBA8gBAPgBAZgCCKAC2AXCAgoQABiwAxjWBBhHwgIGEAAYBxgewgIIEAAYCBgNGB7CAgsQABiABBiGAxiKBcICCBAAGIAEGKIEwgIIECEYoAEYwwSYAwCIBgGQBgiSBwMyLjagB6kk&sclient=gws-wiz-serp)

Docker (Latest). Docker is required for the local translation option to work. You can download and install uusing the link below. Note this installation can take some time and may require a restart of your computer. Use default settings when installing Docker and make sure you open and run docker for windows UI before attempting local translation. The program will automatically install the required container and language packs.

 - [Docker](https://docs.docker.com/desktop/install/windows-install/)

## **Instructions**

#### Step 1

Open up T4C. Press Ctrl+I and go to the options tab. Check all log options to on, including NPC interaction and make sure that the main log option is enabled. This is a critical step to the entire program working. If you do not enable these options, the program will not be able to read the log file and translate it. Once you have enabled these options, close the game client and proceed to the next step.

#### Step 2 (Optional)

Configure and enable the Translation API in Google Cloud (Save your API key for later, you will need to enter it into the program), for a detailed guide, see here: https://crmsupport.freshworks.com/support/solutions/articles/50000004404-google-translate-key this option will give you the best supported translation, the other option is to install and use libre translate which requires more setup and is not as accurate as the google translate API. To install libre translate and use the local translation feature, follow the instructions in the "Option Preparation" section above.

**NOTE:** **Google requires you to enable billing to use the API. After a free limit, which is fairly high, they charge money for using their services after. I think it is like 20$ after this for every million characters, but please read up on the google translate documentation for official information, DO NOT RELY ON ME. I’ve never actually had to pay anything, especially with this as once you enable billing you get a large free credit limit that would cover it even if you did but be careful. If you abuse this you may be charged, and I am not responsible for that if you do so you have been warned and decide if you want to do this or not. As part of using the API you will need to enable billing in the cloud console. There are alternatives, but local translation is still pretty bad compared to these cloud translation services. **

#### Step 3
Launch T4C_Translator.exe

#### Step 4
Select your "Source Folder". This is the location of your T4C Logs. The default directory will be "C:\Users\yourusername\AppData\Local\Dialsoft\Logs"

#### Step 5
Select your "Translate From", language. The default is French "fr"

#### Step 6
Select your "Translate To", language. The default is English "en"

#### Step 7
Select your "Translated File", this is the file that the translated logs will write to as you interact with the game client. Default is "\TranslatedLogs.txt"

#### Step 8
Enter your "Google Translate API Key" or use the Translate Locally checkbox. You should have set this up in step 2, and should be a string of characters. Please ensure it is a valid key, the API is enabled in the google console and that you have enabled billing, as the API may not work without it enabled (I have not tested this recently so it may work without). 
Optionally, you can leave this field blank and click the "Translate Locally" checkbox to use the local translation feature and libre translate. If you are using the local translation feature, you will need to have Docker installed and running, and the docker image will automatically install and run. Note this may take at least 10 minutes, please be patient the program status bar will update to show when installation is complete and will start translating after that is done. This feature is not as accurate as the google translate API, but it is free and does not require an API key."

#### Step 9
Press the "Save Settings" button, this will save your setting in the "config.json" file in your T4C_Translator directory.

#### Step 10
Press the "Start Translation" button. The button should now change to "Stop Translating", this means the program is running and waiting to translate the log.

#### Step 11
Open T4C and start playing. Translated output should now be shown in the output text window in the program. Optionally, to view the translated log file directly, open the translated text file location selected in step 7, Default is "\TranslatedLogs.txt" in Notepad++ (Important!)

#### Step 12
This step is optional, only if you are viewing the log manually to get real-time updates, in Notepad++ open the "View" menu at the top bar and enable "Word Wrap" and "Monitoring Tail -f". This will wrap text as well as automatically keep the file refreshed in the background, optionally you can select Setting->Preferences->MISC and enable the "Scroll to the last line after update" option to keep the file auto focused on the latest translated line.

#### Step 13
You can now also use the manual translation options to the right of the setting window. This will allow you to manually translate text from source to output language, which will then automatically copy the translated text to your clipboard for pasting in the game client or other applications and being able to speak with players over a language barrier without having to manually use google translate. You can also manually click the "Copy" button to copy to the clipboard.

#### Step 14
The original untranslated text will show above the translated text, you can read text from players or NPCs in the translated language selected, as well as copy/paste NPC keywords in the original language which should be wrapped in quotes. Keywords will automatically be in white and can be clicked to copy to your clipboard. Words of interest are gray like the game client. Once a word is manually translated, it can easily be added into your quick translation list by clicking the button. This will add it to your list and any time you select this word from the quick translate list, it will copy the original language to your clipboard. This feature is great for communicating with other players across language barriers. Chat channels can be added to your ignore list and saved, then if you toggle the ignore chat option, any channels in your ignore list will not be translated. This can be useful when wanting to save on API calls.

## Troubleshooting

#### Local Translation Error:
If the docker container installed but still translation is not working, language packs may not have finished installation in time. Please open docker for windows and find the libre translate container, if it is still installing, wait for it to finish and try again. If it is not installing, click on the port number in the UI and see if the web version loads. If you dont see the webpage, that means language packs are still installing. Close the translator app and wait a few minutes until your able to load the libre translate web page, then try again. If it still does not work, try restarting your computer and try again. If it still does not work, please contact me on the T4C discord or GitHub.
