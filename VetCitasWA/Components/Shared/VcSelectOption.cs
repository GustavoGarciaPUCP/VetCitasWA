namespace VetCitasWA.Components.Shared
{
    // Opción para el componente VcSelect (dropdown personalizado)
    public class VcSelectOption<TValue>
    {
        public TValue Value { get; set; } = default!;
        public string Label { get; set; } = "";
        public bool Disabled { get; set; }

        public VcSelectOption() { }

        public VcSelectOption(TValue value, string label, bool disabled = false)
        {
            Value = value;
            Label = label;
            Disabled = disabled;
        }
    }
}
