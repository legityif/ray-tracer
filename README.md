## Ray Tracer

## Rendering images
```
dotnet run -- -f tests/final_scene.txt -o output.png -p "0,-0.1,0" -a "1,1,0" -n -5 -r 0.04 -t 1.45 -x 4 -w 800 -h 800
```

## Sample outputs

###### Sample 1 

```
dotnet run -- -f tests/sample_scene_1.txt -o images/sample_scene_1.png -x 4
```

<p float="left">
  <img src="images/sample_scene_1_s1.png" />
  <img src="images/sample_scene_1_s2.png" /> 
</p>

###### Sample 2

```
dotnet run -- -f tests/sample_scene_2.txt -o images/sample_scene_2.png -x 4
```

<p float="left">
  <img src="images/sample_scene_2_s1.png" />
  <img src="images/sample_scene_2_s2.png" /> 
</p>

## References 
Intersections

https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/minimal-ray-tracer-rendering-spheres

Shadows & Diffusion

https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/ligth-and-shadows
https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/diffuse-lambertian-shading
https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/shading-multiple-lights

Reflection and Refraction

https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
https://blog.demofox.org/2017/01/09/raytracing-reflection-refraction-fresnel-total-internal-reflection-and-beers-law/
https://www.researchgate.net/figure/Principle-of-the-Fresnel-effect-the-amount-of-reflection-on-a-reflective-surface-depends_fig3_319178578
https://stackoverflow.com/questions/29758545/how-to-find-refraction-vector-from-incoming-vector-and-surface-normal#:~:text=Let%20n%20be%20the%20normalized,%2D%20sqrt(1%2DMath.

Random double between range

https://code-maze.com/csharp-random-double-range/

Glossy Materials (Phong illumation model)

https://www.scratchapixel.com/lessons/3d-basic-rendering/phong-shader-BRDF/phong-illumination-models-brdf
https://stackoverflow.com/questions/37798976/java-ray-tracing-glossy-reflection-coloring
https://stackoverflow.com/questions/32077952/ray-tracing-glossy-reflection-sampling-ray-direction

Depth of Field

https://pathtracing.home.blog/depth-of-field/
https://medium.com/@elope139/depth-of-field-in-path-tracing-e61180417027
https://computergraphics.stackexchange.com/questions/66/how-is-depth-of-field-implemented

Custom Camera

https://math.stackexchange.com/questions/40164/how-do-you-rotate-a-vector-by-a-unit-quaternion
https://en.wikipedia.org/wiki/Quaternion#Hamilton_product


