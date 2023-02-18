
public class Define 
{
    public enum MoveDir
    {
        Idle,
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }

    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game
    }

    public enum Sound
    {
        Bgm,
        Effect,
        Max
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
}
