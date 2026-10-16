import React from 'react';

interface CheckboxProps {
  checked: boolean;
  OnChange: (checked: boolean) => void;
  ariaLabel: string;
  disabled?: boolean;
  onClick?: React.MouseEventHandler<HTMLInputElement>;
}

export const Checkbox = ({
  checked,
  OnChange: onChange,
  ariaLabel,
  disabled,
  onClick,
}: CheckboxProps) => (
  <label className="xp-checkbox" data-checked={checked}>
    <input
      aria-label={ariaLabel}
      checked={checked}
      disabled={disabled}
      onChange={(event) => onChange(event.target.checked)}
      onClick={onClick}
      type="checkbox"
    />
    <span className="xp-checkboxMark" />
  </label>
);
