using System;
using System.Windows.Media.Animation;

namespace SharedProject1.AssistImpl
{
    public class LayeredAnimation
    {
        public readonly string Name;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer0;
        public readonly ObjectAnimationUsingKeyFrames Visibility0;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer1;
        public readonly ObjectAnimationUsingKeyFrames Visibility1;
        public readonly int MaxLayers;

        public LayeredAnimation(string name, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer0,
            ObjectAnimationUsingKeyFrames visibility0, 
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer1,
            ObjectAnimationUsingKeyFrames visibility1,
            int animMaxLayers)
        {
            Name = name;
            Layer0 = layer0;
            Visibility0 = visibility0;
            Layer1 = layer1;
            Visibility1 = visibility1;
            MaxLayers = animMaxLayers;
            
        }
    }
}