
# Arcatos
A text-based RPG engine built in C# by a non-developer. The development of this is inspired by all the MUDs I played as 
a kid, and will allow for a good sandbox feel. I won't add graphics because I can't draw, but I am going to try and add
some support for adding images in case you want to. What I can promise is that this is going to be an incredibly 
convoluted system. I can also promise that updates and contributions will be sporadic. 

## The Plan
[Roadmap](Roadmap.md) - I have a concept of a plan

Arcatos is going to be primarily a console app, but I do have some ambition of making a gui for it using something like
Avalonia, but only to make a virtual "terminal" so I don't have to account for all the different terminal apps that
people use (and Microsoft's weird new terminal app that you can't resize). 

This will be an engine designed in such a way that you really only have to add content in JSON files as well as engine
complexity settings (to disable or enable different features). 

## Features
- Movement
    - Narrative "scenes" connected with 10 directions: Up, Down, and the Cartesians (my favorite reality show)
    - Mapspaces with specific entry and exit points for the ability to make a massive world.
- Interactivity
    - Use our character's five senses to almost everything.
    - Let people figure things out for themselves or guide them through the game.
    - All NPCs can have conversations in a Morrowind-style dialogue system (idk how even to do this)
- Other
    - Narrative Character Customization
    - Customizable and Craftable Items

## Disclaimer
I am not a developer, this is something I am doing for fun when I feel like it. I will try for consistency, but also 
not getting burnt out. I will try for consistency and good code standards but my priorities in the beginning are 
readability, brevity, and of course functionality. I will still take any suggestions or (constructive) feedback gladly,
as I'm taking this at least a little seriously.