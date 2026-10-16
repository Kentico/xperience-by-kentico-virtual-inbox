import React from 'react';
import { cn } from '../../lib/utils';

interface InputProps extends Omit<
  React.InputHTMLAttributes<HTMLInputElement>,
  'onChange'
> {
  OnChange: (value: string) => void;
  startIcon?: React.ReactNode;
  endIcon?: React.ReactNode;
}

export const Input = ({
  className,
  OnChange: onChange,
  startIcon,
  endIcon,
  ...props
}: InputProps) => (
  <div className={cn('xp-inputWrap', className)}>
    {startIcon && (
      <span className="xp-inputIcon xp-inputIconStart">{startIcon}</span>
    )}
    <input
      className="xp-input"
      onChange={(event) => onChange(event.target.value)}
      {...props}
    />
    {endIcon && <span className="xp-inputIcon xp-inputIconEnd">{endIcon}</span>}
  </div>
);
