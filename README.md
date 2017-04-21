# Disculator
Disc Priest calculator for WoW


Hello, my esteemed Priestly friends!

I've been plugging away for the last couple of weeks at a calculator app to try and help min-max Disc priests. It's getting pretty close to the point where it can actually spit out some useful data, so I'm hoping to get some more eyes on it now. I switched to a C# calculator app because my Spreadsheet-Fu wasn't up to the task of accurately modeling Atonement stacking, and normal computer code is a lot easier to scrutinize and debug than spreadsheets anyway.

The code is here:
https://github.com/Debuggernaut/Disculator

If you're on Windows 10, you can download and use Visual Studio Community for free from here to compile it:

https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15

(the VS community installer can install Unity and Unreal Engine too while it's going, so if you've been meaning to play around with game development, you should go for it now! And help me get this app working too!)

What Works:

    Raw healing per second, DPS, Mana per second, healing per mana, etc for all damage and healing spells
    Simulates a number of healing styles, such as letting Atonement fall off completely, refreshing it at 4 stacks, using/not using Penance, etc
    Helps you put together a mana budget given the fight length and number of Blessings of Wisdom you have
    I painstakingly calculated exactly how Mindbender/Shadowfiend works over the course of an hour or two in the Priest hall swapping gear around and casting it: It rounds up to the next swing, so if you have enough haste to make it swing 9.1 times, it'll swing exactly 10 times



Works in Progress:

    No support for tentative PTR changes (going to work on this ASAP once the live calculator is mostly done)
    It doesn't try to model any set bonuses yet
    It assumes you have an 875 Darkmoon Deck equipped, and the way it calculates the Darkmoon Deck's effect is pretty lazy and maybe inaccurate
    For the rotation modeler and mana budget helper, it assumes you're using Shield Discipline and that Shield Discipline is being activated on every single Power Word: Shield cast
    I haven't yet modeled our signature move: stacking Atonement on the whole raid and casting Light's Wrath + other DPS spells. For now, you can probably estimate that combo's healing yourself as "Raid Size * Average Raider Health", haha
    I haven't modeled using our cooldowns interspersed with normal healing
    I haven't yet modeled any legendaries
    There's no easy way to compare the effects of our traits, other than to manually set them each time
