using NarrativeGraphTool.Data;
using UnityEngine;

namespace NarrativeGraphTool.Samples
{
    /// <summary>
    /// Common emotions for a dialogue line.
    /// Extend or replace this enum to match your game's art direction.
    /// </summary>
    public enum Emotion
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Surprised,
        Fearful,
        Disgusted,
        Thinking,
        Embarrassed,
    }

    /// <summary>
    /// Example metadata asset for games that use character portraits and emotions.
    ///
    /// <para><b>How to use:</b></para>
    /// <list type="number">
    ///   <item>Right-click in the Project window → Create → Narrative Graph → Emotion Line Metadata.</item>
    ///   <item>Fill in the fields (emotion, portrait sprite, optional voice clip).</item>
    ///   <item>Assign the asset to a LineNode or LineBlock's Metadata field in the graph.</item>
    ///   <item>At runtime, cast <c>line.metadata</c> to <c>EmotionLineMetadata</c> and read the fields.</item>
    /// </list>
    ///
    /// <para><b>Runtime example:</b></para>
    /// <code>
    /// runner.OnLine += line => {
    ///     dialogueText.text = line.text;
    ///
    ///     if (line.metadata is EmotionLineMetadata em) {
    ///         portraitImage.sprite = em.portrait;
    ///         emotionLabel.text    = em.emotion.ToString();
    ///
    ///         if (em.voiceClip != null)
    ///             audioSource.PlayOneShot(em.voiceClip);
    ///     }
    /// };
    /// </code>
    ///
    /// <para>
    /// Copy this file into your own game project and customise it freely —
    /// add a <c>Color</c> tint, a camera shake preset, an animation trigger, anything you need.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "Narrative Graph/Emotion Line Metadata", fileName = "EmotionMetadata")]
    public class EmotionLineMetadata : NarrativeLineMetadata
    {
        [Header("Character")]

        [Tooltip("Emotion displayed on the character's portrait.")]
        public Emotion emotion = Emotion.Neutral;

        [Tooltip("Portrait sprite shown in the dialogue UI for this line.")]
        public Sprite portrait;

        [Header("Audio")]

        [Tooltip("Voice clip for this line. Leave null if not using voice acting.")]
        public AudioClip voiceClip;
    }
}
