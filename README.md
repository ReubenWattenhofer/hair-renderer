# Hair Renderer
Undergraduate senior project

## Mentors:
Kalle Bladin, USC Institute for Creative Technologies  
Dr. Hans Dulimarta, Grand Valley State University

## Project Description  
Hair modeling is generally divided into three components: geometric modeling, rendering, and animating/simulating.  This project focuses on hair rendering.  Current features include Kajiya-Kay lighting with ambient lighting, deep opacity maps by Yuksel & Keyser, and a partial implementation of occupancy maps by Sintorn & Assarsson.  For a more complete description of the project, see the [report](https://github.com/ReubenWattenhofer/hair-renderer/blob/master/Report.docx).

## Using This Project  
You will need to use an external hair model, since none are included in this project.  The Unity Asset Store might be a good place to find one.  This project is currently designed for ribbon patch hair models.  In addition to the hair model, you will need textures, hopefully provided with the model.  These include diffuse and alpha textures.  Default specular shift and secondary highlight noise textures are provided by this project (sourced from [here](http://web.engr.oregonstate.edu/~mjb/cs519/Projects/Papers/HairRendering.pdf "SIGGRAPH slides")).

Download the "hair-renderer" Unity package, which includes the shader, C# scripts, and textures.  Open your current Unity project and select Assets/Import Package/Custom Package.  Select the hair-renderer package and import all.

Apply the shader to each hair mesh material (the shader is Custom/basic).  Add the MeshSorter.cs script to the immediate parent of the hair meshes.  This parent must not have any children other than the hair meshes.  if your hair model does not separate each ribbon patch into a separate mesh, then MeshSorter will not work.  (Note: as of 3/15/2019 it is not working anyway)
