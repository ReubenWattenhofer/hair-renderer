# Hair Renderer
Undergraduate senior project

## Mentors:
Kalle Bladin, USC Institute for Creative Technologies  
Dr. Hans Dulimarta, Grand Valley State University

![Result](https://github.com/ReubenWattenhofer/hair-renderer/blob/master/journal-files/renderer_all_features.PNG)  

## Project Description  
Hair modeling is generally divided into three components: geometric modeling, rendering, and animating/simulating.  This project focuses on hair rendering.  Current features include Kajiya-Kay lighting with ambient lighting, self-shadowing using deep opacity maps by Yuksel & Keyser, and transparency using a partial implementation of occupancy maps by Sintorn & Assarsson.  For a more complete description of the project, see the [report](https://github.com/ReubenWattenhofer/hair-renderer/blob/master/Report.docx).

## Using This Project  
You will need to use an external hair model, since none are included in this project due to licensing.  The Unity Asset Store might be a good place to find one.  This project was tested using a ribbon-patch hair model, but should work for any hair model.

In addition to the hair model, you will need textures, hopefully provided with the model.  These include diffuse and alpha textures.  Default specular shift and secondary highlight noise textures are provided by this project (sourced from [here](http://web.engr.oregonstate.edu/~mjb/cs519/Projects/Papers/HairRendering.pdf "SIGGRAPH slides")).

Download the "hair-renderer" Unity package, which includes the shader, C# scripts, and textures.  Open your current Unity project and select Assets/Import Package/Custom Package.  Select the hair-renderer package and import all.

Apply the "BackgroundHairCombine" material (under Resources/Hair_Renderer/Materials/Transparency) to the hair mesh.  If the hair model is composed of individiual meshes for each ribbon patch, create a parent object and add the meshes to it (though this will most likely already be done).  Make sure the parent object has the following components:
  * A mesh renderer, with the "BackgroundHairCombine" material assigned to it.
  * A mesh filter
  * MeshMerge.cs script, found under Hair_Renderer/Scripts

MeshMerge.cs will merge the meshes into a single mesh owned by the parent.  This parent must not have any children other than the hair meshes.

For the self-shadowing and transparency to work, additional work must be done.  For self-shadowing, create a camera and make sure its projection is set to orthographic.  Set the camera size to 20, and make sure the camera preview does not clip the hair.  Assign a directional light as a child of the camera, and make sure both the light and camera are pointing the same way (ie light transform is the identity matrix).  Lastly, add the DeepOpacity.cs script to the camera, and customize the public variables as you see fit.  The camera, and by extension the light, can be easily set up by pressing ctrl+shift+f.

For transparency, attach the TransparencySorting.cs script to the main camera, and set the public variables.
