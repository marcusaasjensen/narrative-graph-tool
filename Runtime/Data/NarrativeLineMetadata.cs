using UnityEngine;

namespace NarrativeGraphTool.Data
{
    /// <summary>
    /// Abstract base for all game-specific narrative line metadata assets.
    /// <para>
    /// Subclass this in your game project to attach typed data to any narrative line node:
    /// </para>
    /// <code>
    /// [CreateAssetMenu(menuName = "My Game/Line Metadata")]
    /// public class MyLineMetadata : NarrativeLineMetadata
    /// {
    ///     public string emotion;
    ///     public Sprite portrait;
    ///     public AudioClip voiceLine;
    /// }
    /// </code>
    /// <para>
    /// At runtime, cast to your concrete type:
    /// </para>
    /// <code>
    /// runner.OnLine += line => {
    ///     if (line.metadata is MyLineMetadata m) {
    ///         portraitImage.sprite = m.portrait;
    ///         audioSource.clip = m.voiceLine;
    ///     }
    /// };
    /// </code>
    /// </summary>
    public abstract class NarrativeLineMetadata : ScriptableObject { }
}
