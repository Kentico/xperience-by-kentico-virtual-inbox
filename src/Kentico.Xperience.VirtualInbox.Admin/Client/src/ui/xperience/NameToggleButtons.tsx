import React from 'react';

interface ToggleItem {
  id: string;
  label: string;
}

interface NameToggleButtonsProps {
  items: ToggleItem[];
  selectedItemId: string;
  OnChange: (id: string) => void;
}

export const NameToggleButtons = ({
  items,
  selectedItemId,
  OnChange: onChange,
}: NameToggleButtonsProps) => (
  <div className="xp-toggleGroup" role="tablist">
    {items.map((item) => (
      <button
        key={item.id}
        aria-selected={selectedItemId === item.id}
        className="xp-toggleItem"
        data-active={selectedItemId === item.id}
        onClick={() => onChange(item.id)}
        role="tab"
        type="button"
      >
        {item.label}
      </button>
    ))}
  </div>
);
