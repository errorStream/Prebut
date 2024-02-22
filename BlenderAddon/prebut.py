"""Prebut: Pre-rendered background utils.

A blender add-on with tools to automate game asset creation for pre-rendered
backgrounds.
"""

import bpy
import json

bl_info = {
    "name": "Prebut",
    "author": "ErrorStream",
    "version": (0, 0, 1),
    # TODO: Lower blender version.
    # Once functionality is fleshed out find min version where it works.
    "blender": (4, 0, 0),
    "location": "3D Viewport > Sidebar > Prebut",
    "description":
    "A tools to automate game asset creation for pre-rendered backgrounds.",
    "category": "Render",
}

export_group_node_name = "Prebut Texture Export"

class PREBUT_OT_add_or_configure_export_node(bpy.types.Operator):
    bl_label = "Configure Export Node"
    bl_idname = "prebut.add_or_configure_export_node"

    def execute(self, context):

        # Find node with name "Prebut Texture Export" or make it
        group_node = bpy.data.node_groups.get(export_group_node_name)
        if group_node is None:
            group_node = init_export_group_node(context)

        configure_export_group_node(context, group_node)

        return {'FINISHED'}

class PREBUT_OT_export_scene_and_data(bpy.types.Operator):
    bl_label = "Export Scene and Data"
    bl_idname = "prebut.export_scene_and_data"

    def execute(self, context):

        base_path = context.scene.prebut_properties.export_directory

        scene_path = bpy.path.abspath(base_path + "scene.fbx")
        bpy.ops.export_scene.fbx(filepath=scene_path,
                                 use_selection=False)

        is_orthographic = context.scene.camera.data.type == 'ORTHO'
        extra_data = {
            "is_orthographic": is_orthographic,
            "orthographic_scale": context.scene.camera.data.ortho_scale if is_orthographic else None
        }
        extra_data_path = bpy.path.abspath(base_path + "extra_data.json")
        with open(extra_data_path, 'w') as outfile:
            json.dump(extra_data, outfile, indent=4)

        return {'FINISHED'}

def init_export_group_node(context):
    bpy.context.scene.use_nodes = True

    group_node = bpy.data.node_groups.new(export_group_node_name, 'CompositorNodeTree')

    group_in = group_node.nodes.new('NodeGroupInput')
    group_in.location = (0, 0)

    group_node.interface.new_socket(name='Image', in_out='INPUT', socket_type='NodeSocketColor')
    group_node.interface.new_socket(name='Depth', in_out='INPUT', socket_type='NodeSocketFloat')

    node = context.scene.node_tree.nodes.new('CompositorNodeGroup')
    node.node_tree = bpy.data.node_groups[export_group_node_name]
    node.use_custom_color = True
    node.color = (0.3, 0.5, 0.6)

    return group_node


def configure_export_group_node(context, group_node):
    base_path = context.scene.prebut_properties.export_directory
    clip_near = context.scene.camera.data.clip_start
    clip_far = context.scene.camera.data.clip_end

    group_node.nodes.clear()

    space = 300

    group_in = group_node.nodes.new('NodeGroupInput')
    group_in.location = (0, 0)
    color_image_save_node = group_node.nodes.new(type='CompositorNodeOutputFile')
    color_image_save_node.base_path = base_path
    color_image_save_node.file_slots[0].path = "color"
    color_image_save_node.location.x = space
    color_image_save_node.location.y = space
    depth_image_save_node = group_node.nodes.new(type='CompositorNodeOutputFile')
    depth_image_save_node.base_path = base_path
    depth_image_save_node.file_slots[0].path = "depth"
    fmt = depth_image_save_node.format
    fmt.file_format = 'OPEN_EXR'
    fmt.color_mode = 'RGB'
    fmt.color_depth = '16'
    fmt.color_management = 'OVERRIDE'
    fmt.linear_colorspace_settings.name = 'Non-Color'
    depth_image_save_node.location.x = space * 3
    depth_image_save_node.location.y = 0

    clip_near_subtract_node = group_node.nodes.new(type="CompositorNodeMath")
    clip_near_subtract_node.operation = 'SUBTRACT'
    clip_near_subtract_node.inputs[1].default_value = clip_near
    clip_near_subtract_node.location.x = space * 1
    clip_near_subtract_node.location.y = 0

    clip_far_divide_node = group_node.nodes.new(type="CompositorNodeMath")
    clip_far_divide_node.operation = 'DIVIDE'
    clip_far_divide_node.inputs[1].default_value = clip_far - clip_near
    clip_far_divide_node.location.x = space * 2
    clip_far_divide_node.location.y = 0

    group_node.links.new(group_in.outputs[0],
                         color_image_save_node.inputs[0])
    group_node.links.new(group_in.outputs[1],
                         clip_near_subtract_node.inputs[0])
    group_node.links.new(clip_near_subtract_node.outputs[0],
                         clip_far_divide_node.inputs[0])
    group_node.links.new(clip_far_divide_node.outputs[0],
                         depth_image_save_node.inputs[0])


class PrebugProperties(bpy.types.PropertyGroup):
    """Properties for persitant storage of add-on settings."""

    export_directory: bpy.props.StringProperty(
        default="//",
        name="Export Directory",
        subtype='DIR_PATH'
    )


# class PREBUT_OT_SetupAndRender(bpy.types.Operator):
#     """Sets up the compositor and renders"""
#     bl_idname = "prebut.setup_and_render"
#     bl_label = "Setup and Render"

#     def execute(self, context):
#         base_path = context.scene.prebut_properties.export_directory
#         clip_near = context.scene.camera.data.clip_start
#         clip_far = context.scene.camera.data.clip_end

#         context.scene.view_layers["ViewLayer"].use_pass_z = True

#         context.scene.use_nodes = True
#         tree = context.scene.node_tree
#         tree.nodes.clear()

#         space = 300

#         render_layers_node = tree.nodes.new(type='CompositorNodeRLayers')
#         render_layers_node.location.x = 0
#         render_layers_node.location.y = 0
#         composite_node = tree.nodes.new(type='CompositorNodeComposite')
#         composite_node.location.x = space
#         composite_node.location.y = space * 2

#         color_image_save_node = tree.nodes.new(type='CompositorNodeOutputFile')
#         color_image_save_node.base_path = base_path
#         color_image_save_node.file_slots[0].path = "color"
#         color_image_save_node.location.x = space
#         color_image_save_node.location.y = space
#         depth_image_save_node = tree.nodes.new(type='CompositorNodeOutputFile')
#         depth_image_save_node.base_path = base_path
#         depth_image_save_node.file_slots[0].path = "depth"
#         fmt = depth_image_save_node.format
#         fmt.file_format = 'OPEN_EXR'
#         fmt.color_mode = 'RGB'
#         fmt.color_depth = '16'
#         fmt.color_management = 'OVERRIDE'
#         fmt.linear_colorspace_settings.name = 'Non-Color'
#         depth_image_save_node.location.x = space * 3
#         depth_image_save_node.location.y = 0

#         clip_near_subtract_node = tree.nodes.new(type="CompositorNodeMath")
#         clip_near_subtract_node.operation = 'SUBTRACT'
#         clip_near_subtract_node.inputs[1].default_value = clip_near
#         clip_near_subtract_node.location.x = space * 1
#         clip_near_subtract_node.location.y = 0

#         clip_far_divide_node = tree.nodes.new(type="CompositorNodeMath")
#         clip_far_divide_node.operation = 'DIVIDE'
#         clip_far_divide_node.inputs[1].default_value = clip_far - clip_near
#         clip_far_divide_node.location.x = space * 2
#         clip_far_divide_node.location.y = 0

#         tree.links.new(render_layers_node.outputs[0],
#                        composite_node.inputs[0])
#         tree.links.new(render_layers_node.outputs[0],
#                        color_image_save_node.inputs[0])
#         tree.links.new(render_layers_node.outputs[2],
#                        clip_near_subtract_node.inputs[0])
#         tree.links.new(clip_near_subtract_node.outputs[0],
#                        clip_far_divide_node.inputs[0])
#         tree.links.new(clip_far_divide_node.outputs[0],
#                        depth_image_save_node.inputs[0])

#         # Trigger the render
#         bpy.ops.render.render('INVOKE_DEFAULT')

#         scene_path = bpy.path.abspath(base_path + "scene.fbx")
#         bpy.ops.export_scene.fbx(filepath=scene_path,
#                                  use_selection=False)

#         print(context.scene.camera.data.type)
#         is_orthographic = context.scene.camera.data.type == 'ORTHO'
#         extra_data = {
#             "is_orthographic": is_orthographic,
#             "orthographic_scale": context.scene.camera.data.ortho_scale if is_orthographic else None
#         }
#         extra_data_path = bpy.path.abspath(base_path + "extra_data.json")
#         with open(extra_data_path, 'w') as outfile:
#             json.dump(extra_data, outfile, indent=4)

#         return {'FINISHED'}

class PT_prebut_panel(bpy.types.Panel):

    bl_region_type = "UI"

    bl_category = "Prebut"
    bl_label = "Prebut"

    def draw(self, context):
        # row = self.layout.row()
        # row.operator("prebut.setup_and_render", text="Setup and Render")
        self.layout.prop(context.scene.prebut_properties, "export_directory")
        self.layout.operator("prebut.add_or_configure_export_node")
        self.layout.operator("prebut.export_scene_and_data")
        self.layout.operator("prebut.enable_depth_layer")

class VIEW3D_PT_prebut_panel(PT_prebut_panel):
    bl_space_type = "VIEW_3D"

class NODE_EDITOR_PT_prebut_panel(PT_prebut_panel):
    bl_space_type = "NODE_EDITOR"

class PREBUT_OT_enable_depth_layer(bpy.types.Operator):
    bl_idname = "prebut.enable_depth_layer"
    bl_label = "Enable Depth Layer"

    def execute(self, context):
        context.scene.view_layers["ViewLayer"].use_pass_z = True
        return {'FINISHED'}

def register():
    """Register add-on."""
    bpy.utils.register_class(PrebugProperties)
    bpy.types.Scene.prebut_properties = bpy.props.PointerProperty(type=PrebugProperties)
    # bpy.utils.register_class(PREBUT_OT_SetupAndRender)
    bpy.utils.register_class(PREBUT_OT_add_or_configure_export_node)
    bpy.utils.register_class(VIEW3D_PT_prebut_panel)
    bpy.utils.register_class(NODE_EDITOR_PT_prebut_panel)
    bpy.utils.register_class(PREBUT_OT_enable_depth_layer)
    bpy.utils.register_class(PREBUT_OT_export_scene_and_data)


def unregister():
    """Unregister add-on."""
    bpy.utils.unregister_class(PrebugProperties)
    bpy.utils.unregister_class(NODE_EDITOR_PT_prebut_panel)
    bpy.utils.unregister_class(VIEW3D_PT_prebut_panel)
    # bpy.utils.unregister_class(PREBUT_OT_SetupAndRender)
    bpy.utils.unregister_class(PREBUT_OT_add_or_configure_export_node)
    bpy.utils.unregister_class(PREBUT_OT_enable_depth_layer)
    bpy.utils.unregister_class(PREBUT_OT_export_scene_and_data)
    del bpy.types.Scene.prebut_properties


if __name__ == "__main__":
    register()
