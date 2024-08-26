namespace Starchives;

public class SharedService
{
	public string Title { get; private set; } = "Default Title";

	public event Action OnChange;

	public void SetTitle(string title)
	{
		Title = title;
		NotifyStateChanged();
	}

	private void NotifyStateChanged() => OnChange?.Invoke();
}
