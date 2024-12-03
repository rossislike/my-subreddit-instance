public class StateManager
{
    private readonly Dictionary<string, string> _states = new();

    public void SaveState(string state, string value)
    {
        if (_states.ContainsKey(state)) _states[state] = value;
        else _states.Add(state, value);
    }

    public bool IsValidState(string value) 
    {
        return _states.ContainsValue(value);
    }

    public string GetStateValue(string state)
    {
        if (_states.TryGetValue(state, out var value))
        {
            return value;
        }
        return string.Empty;
    }
}