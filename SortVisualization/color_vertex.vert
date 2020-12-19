#version 460 core

layout (location = 0) in vec4 position;

layout (location = 10) uniform mat4 modelview;
layout (location = 11) uniform mat4 projection;

layout (location = 20) uniform vec4 color;

out vec4 vs_color;

void main(void)
{
	gl_Position = projection * modelview * position;
	vs_color = color;
}