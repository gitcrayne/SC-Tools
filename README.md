# SC-Tools
A collection of tools for Star Conquest that is screenreader and low vision friendly. This is a personal project I made for myself, but suggestions are welcome.


# What Is Included???
1. A TCT-GST converter that takes a base-25 TCT time and turns it into either elapsed time or a calendar date/time. The first is useful for figuring out things like charge, time, or distance-time traveled, while the latter is useful for determining at what time from the current date/time the TCT provided will occur. This is useful for determining station lifetimes, in particular. It can also, though it isn't perfectly accurate, take the output from a chronometer and give you the in-game date/time. Please don't count on this at the moment. It needs some TLC. Note that you cannot use TCT displayed in base-10 from the game. You must have the option to display TCT in base-25 in order to plug the TCT into the tool.
2. A 3D Heading Calculator. Let's say you know the heading from Campeche to Andy's and the heading from Andy's to Tom Town, and you want to figure out a heading from Campeche to Tom Town or Tom Town to Campeche. Plug in the known headings and you will get the third side of the triangle.

# Accessibility
Yes! It's baked into the app. I'm working on some verbosity issues (it's very talkative right now). It is designed from the ground up to be accessible to screen reader users and low vision users. There are some other to-dos such as allowing switching between light and dark theme, but it scales with window size now and is high contrast. More to come!

# To-Do
1. I am working on the accessibility, as mentioned. It needs to be less verbose and able to toggle between dark and light mdoe.
2. I would like for the TCT tool to also go from planetary time to TCT. This is going to take some work due to the need to standardize inputs.
3. I am looking for suggestions. If there is something you would like to see (I considered a quick vector reference, Feng Wo currency conversion, and maybe a glyph color reference and a temp station lifetime calculator. If there is anything else you would like to see, let me know.
