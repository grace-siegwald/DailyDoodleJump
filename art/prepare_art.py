"""
Makes the game's sprite images from the full-size drawings in art/source.

Each drawing is cropped to its visible part and shrunk to a size that suits the game,
then saved to src/DailyDoodle.Web/wwwroot/images with the same file name.
Run it from anywhere after adding or changing a drawing:

    pip install pillow
    python art/prepare_art.py
"""

from pathlib import Path

from PIL import Image

SOURCE = Path(__file__).parent / "source"
OUTPUT = Path(__file__).parent.parent / "src" / "DailyDoodle.Web" / "wwwroot" / "images"

# Width in pixels of each finished image: roughly 3-4 pixels per game unit at the widest the game
# ever draws it (platforms up to 90 units wide, the player 40). Bigger wastes download size and
# makes outlines break up when the game shrinks it; smaller looks blurry on sharp screens.
WIDTHS = {"Player.png": 160}
DEFAULT_WIDTH = 300

# Pixels at least this see-through (0-255 alpha) count as empty when cropping.
EMPTY_ALPHA = 16


def main() -> None:
    sources = sorted(SOURCE.glob("*.png"))
    if not sources:
        raise SystemExit(f"No PNG files found in {SOURCE}")
    if not OUTPUT.is_dir():
        raise SystemExit(f"Output folder not found: {OUTPUT}")

    for path in sources:
        image = Image.open(path).convert("RGBA")
        box = image.getchannel("A").point(lambda a: 255 if a > EMPTY_ALPHA else 0).getbbox()
        if box is None:
            print(f"Skipped {path.name}: it is completely transparent")
            continue

        drawing = image.crop(box)
        width = WIDTHS.get(path.name, DEFAULT_WIDTH)
        height = max(1, round(drawing.height * width / drawing.width))

        # Shrink in premultiplied alpha ("RGBa") so see-through edges don't pick up dark fringes.
        small = drawing.convert("RGBa").resize((width, height), Image.LANCZOS).convert("RGBA")
        small.save(OUTPUT / path.name, optimize=True)
        print(f"{path.name}: {image.width}x{image.height} -> {width}x{height}")


if __name__ == "__main__":
    main()
