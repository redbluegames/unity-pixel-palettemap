Unity-Pixel-Palettemap
================
Pixel Palettemap "palettizes" textures, allowing them to be used with a shader for alternate color schemes and palette swapping.

NOTE This read me is out of date. I will update it when I'm finished with major outstanding features and I've figured out the final workflow.

Overview
================
In games we often want to colorize the same 2D image many different ways. Maybe we want to have several color variants for playable characters, like in Super Smash Brothers. Or maybe we want to let players dye parts of their armor. One solution to this is to draw separate sprite sheets for each color variation, but that has the con of using more memory and more importantly, difficult upkeep. When you want to make a change you have to change every version of the texture. And if we want to allow many combinations, like a custom hat color, custom cape color, and custom armor color, it would be virtually impossible to draw every permutation. You could instead use color tinting on a black and white texture but that will only allow you to make all colors in the image a shade of one color. Maybe you want blonde hair and a blue cape. To solve these issues, we can use a color indexed image.

##Color Indexing and Palettes
A color indexed image is made up of two structures - an indexed image and a palette of all the unique colors in the image. A typical image stores pixels as colors, which consist of 4 bytes, one for each color channel and the alpha value. In a color indexed image, instead of storing color information each pixel points to its corresponding color’s index in a palette of colors. This reduces redundant information resulting in a smaller file size. Typically the palette size is a byte, allowing for 256 unique colors, which is what this tool uses. With each pixel storing only one byte instead of four, a color indexed image should be quarter the size of its uncompressed source image.

![Color Indexing]
(https://docs.google.com/drawings/d/1DGYJIlG9qPU6PZ9Un4MGe50YfI0802PhoML57Rz6ECY/pub?w=666&h=319)

With the color information split out into another structure, you can easily change all colors in the image simply by changing out the palette. This is shown in the next diagram.

![Swapping Palettes A]
(https://docs.google.com/drawings/d/1MQ3mVvWj5AGfNh0hLXJqljNCOePI5OxOAC9_pgCdGCA/pub?w=460&h=347)

##RBPaletteMapper Overview
It is possible to create color indexed images by hand in a drawing program, but it is tedious and prone to error. This tool is designed to help you quickly create a color indexed image from your source image. Given a source image it creates two other texture files: a palette map (the color indexed image) and a palette key. The palette map is the color indexed texture, pointing to coordinates in the palette key. You then create new palettes for the image by copying the generated palette key. The package also contains custom shaders, used to render palettized images as sprites or as textures on meshes. There are also a few sample scripts that show how to swap out palettes at runtime.

Using the Tool and Workflows
=======
###Quick Workflow
Create a palettized image:

1. Create your source texture
  1. Draw everything that you want to color separately as a different color. This makes sure that they get assigned as new entries in the palette
2. Import texture into Unity and set up its settings as follows:
  1. Texture Type: Advanced
  2. Read / Write enabled
  3. Point filter
  4. RGBA32 format
3. Select the PaletteMapper tool under RedBlueTools in the menu bar
4. Drop the texture into the texture slot
5. Click Build Texture to create a PaletteMap texture and PaletteKey texture

For each sprite you want to palettize:

1. Create a new material
2. Select the custom shader for the new material
  1. For a sprite choose RBTools / Palettized Image / PaletteSprite
  2. For a texture with transparency choose RBTools / Palettized Image / Palette Texture (Until/Transparent)
  3. For a texture without transparency choose RBTools / Palettized Image / Palette Texture (Until)
3. If you are needing a new palette for the sprite, duplicate the key that corresponds to the palette map and colorize it how you want in an image editing program
4. Assign the new palette key to the material
5. For a sprite:
  1. Create sprites from your generated PaletteMap texture and use one of those sprites on your game object. You can find more information on creating sprites in the Unity Documentation here.
  2. Assign the new material to the sprite renderer.
6. For a texture:
  1. Assign the palette map into the Material as the Base texture
  2. Assign the newly created palette texture into the Material as the Palette texture

###Creating Palette Swapping
There are many ways to achieve a palette swapping effect. Each method is just a different way of changing out the sprite or texture’s palette, while using the same palette map. You can change the object’s Material to a material that uses another palette, you can change the specific Palette texture used by a particular material, or if you have multiple palettes in one palette image, you can change the texture’s Y offset of your material. I’ve included several scripts that provide examples of this. MaterialTween.cs swaps between materials, PaletteScroll.cs scrolls through the palettes in a multi-palette texture by setting texture offset, and SetTextureVariableExample.cs swaps out the textures supplied as the palette to a material.
	
###Advanced Workflow and Best Practices
The tool works great for quick and dirty palettized sprites. But if you have a large project where the sprites you are using change frequently, you will want to use these advanced workflow recommendations.

1. Create your own palette key based on the source texture. This lets you put the colors in the order you want, so that it is easier to create new palettes. It also helps when you insert or remove colors from the source image. If you insert a color in your original image, a generated key will insert a color somewhere. You would have to figure out where it entered that color and add one to all the palettes you’ve created. By providing your own key, you can insert that color where you want it and immediately do the same to all your palettes. You can also label your custom key to help you with organization. As long as the font you use is made up of colors in the palette, it won’t even create unused colors in the key. Here is a custom palette key I am using in our project at Red Blue Games: TODO: Publish my key

2. Use one texture file for all your color palettes for a specific palette map texture. This lets you modify all palettes easily at one time. For example, if you insert a new color into the palette key, you can easily insert a column of new pixels into all palettes.

To specify which palette to use on your material, you need to use the Texture Offset on the Palette’s texture. The value is 0.0 - 1.0, bottom to top. Divide the index of the row you want by the number of palette rows in the texture to find the y offset. For example, to get the 3rd row from the bottom in a texure with 4 palettes, divide 2 (0 based index) by 4 to get 0.5. The row spans the texture from 0.5 to 0.75 so any number in between the two will give you that palette.

Code Explanation
================
* The tool first creates a palette key from the provided source image. If you supply your own palette key image it still creates a new palette key, but instead of creating a palette key from the source image it uses the supplied palette key image to create a palette key.
  * To create the palette key, the tool simply iterates through the pixels in the source image and inserts unique colors into a palette. If you choose to sort the palette, it will sort the colors after they’ve been identified.
* It then creates the palette map from the source texture using the generated palette key. For each color it finds in the source texture it finds the corresponding index for that color in the palette key. It then writes that new value into the alpha channel of a new texture.
  * The alpha values are normalized to the size of the texture. So a palette with 4 colors will have alpha values from [0, .25), [.25, .5), [.5, .75), and [.75, 1.0f). This lets us keep the palette image small and also makes it so that the content of the palette map is discernable when viewing the alpha channel.
* When it’s finished, it writes both the palette key and the palette map out to disk at the same directory as the source image.

####The Shaders
Each shader simply uses the alpha values at each pixel in the Palette Map to point into coordinates of the Palette. Instead of using the color value from the primary texture, it uses the color values found in the Palette.

FAQ
======
* When I change the Source texture, do I have to update all of the custom alternate palettes that I’ve made?
  * If you add or remove a color to the source image, it can change the order of the colors in the palette since they are sorted by grayscale. Therefore, you have to re-create your palettes if you do this.
    * Note, even if I go to a method of First Color found, first added, it won’t fix this since you could change the order in which the colors are found in the image.
    * The only fix is to support supplying custom palette keys as input. This would let users add new colors to the end of their palette, so that they’d just have to adjust those two colors in all their palettes.
  * Therefore it is recommended that you define the palette beforehand so that it’s unlikely to change.
* What are some uses for this tool?
  * Allows for easily created alternate color schemes for player characters or enemies
  * Allows for players to customize their character
  * Can be used in combination with Palette Swapping for hit flashing or animations
* Why would I use this instead of just creating new textures for each color?
  * Saves texture memory
  * Saves time repainting textures every time you want to change or add animations to an existing character
* Why is it so slow?
  * I did not optimize the algorithms, so it will take a while to convert large textures.
  * I’ve split my character into several spritesheets. How do I create a palette map for them?
You should create your own palette key that includes all the colors from each spritesheet. Then generate a palette map for each spritesheet providing the same palette key to each. This will make sure your indexed values for each palette map point to the same set of colors found in the palette key.

Troubleshooting
==============
* PaletteMap Error: Tried to Add more colors to palette key than are currently supported due to PaletteMap's Alpha8 format
  * This error occurs when either the source texture contains more colors than a palette can store ( > 256 ), or a supplied custom palette key contains more colors than a palette can store. This is typically due to compression, which can be caused from any of the following:
    * If the texture is not a power of 2 in size, make sure it is not flagged to round. (Non Power of 2: None)
    * Filter mode should be point
    * Make sure Max Size is bigger than the texture size
    * Format: RGBA 32 bit

Further Reading
=============

Indexed Colors

http://en.wikipedia.org/wiki/Indexed_color

http://en.wikipedia.org/wiki/Palette_swap

Shaders

http://gamedev.stackexchange.com/questions/43294/creating-a-retro-style-palette-swapping-effect-in-opengl

Attribution
================
Demo world image by Lanea Zimmerman, used under license CC-BY 3.0 (http://creativecommons.org/licenses/by/3.0/)

Get the image here: http://opengameart.org/content/tiny-16-basic
