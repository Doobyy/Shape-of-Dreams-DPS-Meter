public interface IEffectComponent
{
	bool isPlaying { get; }

	bool isLooping => false;

	void Play();

	void Stop();
}
