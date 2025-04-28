namespace Interfaces
{
    public interface IHover
    {
        bool EnableHover { get; set; }
        void OnPointerHoverEnter();
        void OnPointerHoverExit();
    }
}
