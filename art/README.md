# Art

`source/` holds the full-size drawings. They are not part of the website.

The game uses smaller copies in `src/DailyDoodle.Web/wwwroot/images`, made by `prepare_art.py`:
each drawing is cropped to its visible part and shrunk to suit the game. Full-size images look
worse in the game, not better: they get drawn so much smaller than they are that most of their
pixels are skipped, which breaks up the outlines. They are also much bigger to download.

To add or change a drawing:

1. Save the full-size PNG in `source/` (transparent background).
2. Run `pip install pillow` once, then `python art/prepare_art.py`.
3. If it's a new kind of object, give it a width in `WIDTHS` in `prepare_art.py`
   (about 3-4 pixels per game unit at the widest it is drawn).
