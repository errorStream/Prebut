<a name="readme-top"></a>

<br />
<div align="center">
  <h3 align="center">Prebut</h3>
  <p align="center">
    <b><i>Pre</i></b>-rendered <b><i>B</i></b>ackground <b><i>Ut</i></b>ilities
  </p>
  <p align="center">
    A suite of tools for making pre-rendered backgrounds in blender for use in unity.
    <br />
    <br />
    <a href="https://errorstream.itch.io/prebut-demo"><b>View Demo »</b></a>
    <br />
    <br />
    <a href="https://github.com/errorStream/prebut/releases">Download</a>
    ·
    <a href="https://github.com/errorStream/prebut/issues">Report Bug</a>
    ·
    <a href="https://github.com/errorStream/prebut/issues">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#whats-inside">What's inside?</a></li>
      </ul>
      <ul>
        <li><a href="#who-is-this-for">Who is this for?</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li> <a href="#dependencies">Dependencies</a> </li>
        <li>
          <a href="#installation">Installation</a>
          <ul><li><a href="#blender-add-on">Blender Add-on</a></li></ul>
          <ul><li><a href="#unity-package">Unity Package</a></li></ul>
        </li>
      </ul>
    </li>
    <li>
      <a href="#usage">Usage</a>
      <ul>
        <li>
          <a href="#extra-functionality">Extra Functionality</a>
          <ul><li><a href="#custom-clear">Custom Clear</a></li></ul>
          <ul><li><a href="#orthographic-camera-center-controller">Orthographic Camera Center Controller</a></li></ul>
        </li>
      </ul>
    </li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>


<!-- ABOUT THE PROJECT -->
## About The Project

![An example scene generated with prebut](Images/zoomed-out.png)

Prebut is a project to demonstrate a workflow for generation and usage
of pre-rendered backgrounds in unity. This workflow allows the
creation visually sophisticated scenes in blender which require a
non-realtime rendering approach and allow objects in unity to interact
with in realtime.

![Another game which uses this approach](Images/dennis-scrolling-demo.gif)

I'm making this project because I want to explore the artistic avenues
and technical considerations of pre-rendered backgrounds. I admire the
stylistic refinement, compositional intentionality, and fullness of
detail of PS1 era pre-rendered backgrounds. These seem to be
considerations which have lost emphasis in games since the hardware
limitation based motivation for the usage of pre-rendered backgrounds
was alleviated, so I decided to explore the potential of this
approach, while evoking a similar style, using modern tooling, through
this project.

This project works by exporting depth information from your scene in
blender and using that to specify the depth value in unity, which
allows objects in unity to move around pre-rendered objects. You can
then export simplified colliders from blender to allow accurate
collisions with pre-rendered objects.

This project supports both perspective and orthographic cameras,
including the ability to effectively pan around orthographic
scenes. My goal with this project was to make it quite flexible and
minimally interfere with your workflow.

### What's inside?

This project is composed of three parts; a blender add-on, a unity
package, and an explanation of how to use them together.

The blender add-on provides a panel for exporting data about the
current scene and for generating a compositor node for feeding
rendered image data into.

The Unity package provides a component which takes in the data and
textures generated by blender and constructs a pre-rendered background
setup in the scene.

### Who is this for?

This project is for people who are interested in experimenting with
pre-rendered backgrounds in their development projects, but have
struggled with getting a working configuration or are looking for a
smoother workflow. It is also useful for devs who specialize in 3d
visual assets and are looking for unique ways to apply these
techniques for their games. This approach is especially useful for
games with fixed camera angles (survival horror, point and click
adventure, visual novel), games with a retro aesthetic, or games with
an isometric view (grid-based strategy, top down adventure, tower
defense, RPG)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Getting Started

### Dependencies

*Note that this project may work on earlier versions of Blender and Unity, these are just the earliest versions I have tested*

- Unity 2020.3 or later
  - Default render pipeline
  - 16 bit per channel texture support
    - Support for this in webgl builds was added later; in 2021.1
- Blender 4.0.0 or later


### Installation

#### Blender Add-on

1. Open Blender Preferences
2. Click "Install"
3. Navigate to `prebut.py` (under `BlenderAddon` if you cloned the repo) and click "Install Add-On"
4. Click the checkbox next to "Render: Prebut"
5. (Optional) Click the hamburger menu in the bottom left and click
   "Save Preferences". This ensures that the add-on will be enabled on
   subsequent restarts.

#### Unity Package

1. Set up a unity project. Note that this has only been tested with the
   default render pipeline.
2. Open the Unity "Package Manager" window
3. Click the plus in the top right
4. Add the package
   - If you cloned the repo
     1. Click "Add package from disk"
     2. Navigate to the Prebut root directory and select the "package.json" file
   - If you downloaded from releases
     1. Click "Add Package from Tarball"
     2. Navigate to and select the "Prebut Unity Package x.x.x.tgz" file

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

Note that this workflow is quite explicit to make sure it works the
first time, feel free to vary it up once you understand what is going
on.

1. Make the blender assets
    1. Build a scene in blender. 
    2. Open the Prebut panel in the 3D viewport or the node editor
    3. Click "Enable Depth Layer" to enable rendering the depth pass (this
       is not needed if this pass is already enabled)
    4. Configure a directory to save the generated assets to in "Export Directory"
    5. Click "Export Data" (This saves relevant data about your scene to
       your export directory. Click this again if you change camera
       settings)
    6. Click "Configure Export Node" (The add-on should henceforth send
       all relevant configurations to this node on render, but you can
       always click it manually to update the node)
    7. In the compositing node editor, connect `Render Layers/Image` to
       `Prebut Texture Export/Image` and `Render Layers/Depth` to `Prebut
       Texture Export/Depth`. You can add any compositing nodes you want
       between `Render Layers/Image` and `Prebut Texture Export/Image` to
       change the look.
    8. (Optional) Toggle `Properties > Render > Film > Transparent` on
       to have color texture terminate at edge of visible content for
       specifying a clear color in Unity.
    9. Render image. (Make sure you are on a consistent frame,
       otherwise it will change the outputted files name.)
    10. Export simplified version of the scene for use in unity. 
       1. Move camera and main lights to a separate collection if they
          aren't already, henceforth called "Shared"
       2. Make a collection with simplified meshes which will act as the
          colliders for the detailed objects you have in your scene,
          henceforth called "Colliders"
       3. Make it so only Shared and Colliders are Visible
       4. Click `File > Export > FBX`
       5. Toggle on `Include > Limit to > Visible Objects`
       6. Navigate to Export Directory
       7. Click "Export FBX"
2. Make the Unity scene
   1. Move the generated files into Unity
   2. Apply presets to pre-rendered color and depth textures
       1. Select pre-rendered color texture
       2. Click the preset button in the top right of the inspector
          - If your color texture has alpha set "Alpha Source" to
            "Input Texture Alpha" and "Alpha is Transparency" to True
       3. Click "PreRenderedColorTexture"
       4. Do the same with the pre-rendered depth texture and "PreRenderedDepthTexture"
   3. Add the setup component to the scene
      1. Make a root game object named "Setup"
      2. Add the component `Prebut/Setup Pre-Rendered Background`
      3. Set the scene field to the prefab for the exported FBX file to "Scene"
      4. Set the exported color texture, depth texture, and extra data to the corresponding fields
      5. Set a background layer (This should be a layer which is not seen by any other cameras)
      6. If your scene is orthographic you can check "Add Orthographic
         Camera Center Controller" to allow panning around your scene.
      7. Set a background color, which is what content outside the
         visible part of the color texture gets cleared to.
      8. Click "Setup"
          - The other setting can be left default for now
          - When you make a change to any of the referenced data,
            click "Setup" again to update the configuration
            
That should be all you need to do. Hit play then you can move a 3D
object around the scene to see it interact with the pre-rendered
parts. Select "Setup" to see colliders to get an idea of where
everything is.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Extra Functionality

These are chunks of functionality which are not strictly necessary for
the core goal of the system, but they are generally useful enough
when it comes to this approach that I deem it prudent to include them.

#### Custom Clear

This piece of functionality gives the ability to clear the background
with a scrolling repeating pattern. This is a common stylistic tool
used for this approach historically, so I decided to give it official
support. This effect is demonstrated in [the
demo](https://errorstream.itch.io/prebut-demo).

![Animation showing the effect](Images/prebut-demo-scroll-bg.gif)

Once you have a scene with `Prebut/Setup Pre-Rendered Background` component
in it, follow these steps to add custom clearing.

1. Add a game object called "BackgroundClear"
2. Add the component `Prebut/Background Clear`. All settings are
   optional.
3. Add the game object to a layer which only contains this object and
   is not seen by any cameras. Henceforth, called the "Clear Layer"
4. In the `Prebut/Setup Pre-Rendered Background` component toggle
   "Should Clear Colors" to False and add the clear layer to hidden
   layers, then click "Setup"

This component should print warnings on play if there are settings in
the current scene which interfere with it, but that should be enough
to work.

#### Orthographic Camera Center Controller

A common approach with this technique when using an orthographic
camera is zooming in on and panning around a large pre-rendered
scene. It is not trivial to keep the depth and color camera and 3D
scene camera in sync, so to facilitate this; this project has the
"Orthographic Camera Center Controller" component. This is generated
automatically by the `Prebut/Setup Pre-Rendered Background` component
if the pre-rendered scene was generated with an orthographic camera
and "Add Orthographic Camera Center Controller" is checked.

The usage is simple. Set Center to the position in world space which
you want the view to center on. Set Zoom to the zoom level you
want. The camera will automatically move to the appropriate location
to center that point and adjust its zoom level.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ROADMAP -->
## Roadmap

- [x] Perspective Backgrounds
- [x] Orthographic Backgrounds
- [x] Orthographic Panning
- [x] Scrolling Clear
- [ ] Lower Minimum Version
- [ ] Panning across multiple background textures
- [ ] Pre-rendered midground and foreground entities

See the [open issues](https://github.com/errorStream/levers/issues)
for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

If you have a suggestion that would make this better, please fork the
repo and create a pull request. You can also simply open an issue with
the tag "enhancement".

Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- CONTACT -->
## Contact

ErrorStream - [itch.io profile](https://errorstream.itch.io) - errorstream@amequus.com

Project Link: [https://github.com/errorStream/prebut](https://github.com/errorStream/prebut)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Acknowledgments

- [Pre-Rendered Backgrounds
  Aesthetics](https://twitter.com/prbg_aesthetics) for aggregating so
  many examples of this medium, and inspiring me to understand it.
- [SpookyFM/DepthCompositing: Depth Compositing Demo in Unity and
  Blender](https://github.com/SpookyFM/DepthCompositing/) for giving
  me some starting intuitions for understanding this approach.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
