# UiToolkitWorld
Adds 2D world space functionality to Unity's UI Toolkit Document

### Overview
Utilize this script in your Unity projects to convert screen space UI Toolkit trees/documents into your 2D world space.


The MonoBehaviour script allows:
- Using the attached gameObject's transform to change the tree/documents position, z-axis rotation, and scale in 2D world space.
- Scale in world space based on a reference unit size on either horizontal or vertical axis.
- Additional scaling in world space from the attached transform's localscale.
- Bounding box displayed in editor window using debug lines for easy positioning/reference.

#### UI Element in World Space
<img src="https://github.com/BAPCon/UiToolkitWorld/assets/79327706/f7ecebd2-11ee-4282-aa2b-c1fd7118c359" width="350" height="" alt="game view screenshot of UI element in world space">

#### Editor Scene View (of rotated world space panel)
<img src="https://github.com/BAPCon/UiToolkitWorld/assets/79327706/e9bac69d-25a4-4d56-8fe8-ecc9e3b70e23" width="350" alt="scene/editor view screenshoot of UI element in world space">

### Example
1. Your target UI document has a top tree asset with dimensions `(600px, 480px)`.
2. You set the property `unitSize` to 16 and the `referenceDimension` enum to `Horizontal`.
3. The final world space dimensions of the object would be `(16 units, 12.8 units)`
4. If the `localScale.z` of the attached transform was `0.5`, the final world space dimensions would be `(8 units, 6.4 units)`.

#### Important
*Do not* add a `UI Document` component to the `gameObject`, the script attaches this component on initialization and handles instances of the provided `PanelSettings`.


### Installation/Usage
You can copy the script contents to a local script, or pull the repository and install locally.

