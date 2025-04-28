namespace Interfaces
{
    public interface ISelectable
    {
        bool EnableSelect { get; set; }
        void Select();
        void Unselect();
        void Remove();
    }
}