using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhileFalse.Retro3D
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PostProcessLinkRendererAttribute : Attribute
    {
        public readonly Type rendererType;

        public PostProcessLinkRendererAttribute(Type rendererType)
        {
            this.rendererType = rendererType;
        }
    }

    public abstract class PostProcessRenderer
    {
        public abstract int priority { get; }

        public abstract void Setup();

        public abstract void SetupStackComponent(VolumeStack stack);

        public abstract void Render(CommandBuffer cmd, Camera camera);

        public abstract bool ShouldRender();
    }

    public abstract class PostProcessRenderer<T> : PostProcessRenderer where T : VolumeComponent
    {
        protected T _component;

        public override void SetupStackComponent(VolumeStack stack)
        {
            _component = SetComponent(stack);
        }

        private T SetComponent(VolumeStack stack)
        {
            if (stack.components.ContainsKey(typeof(T)))
            {
                return stack.components[typeof(T)] as T;
            }

            return null;
        }
    }
}
