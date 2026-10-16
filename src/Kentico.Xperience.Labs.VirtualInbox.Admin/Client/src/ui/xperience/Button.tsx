import React from 'react';
import { cn } from '../../lib/utils';

type XperienceButtonColor = 'secondary' | 'tertiary' | 'quinary' | 'alert';

interface ButtonProps extends Omit<
  React.ButtonHTMLAttributes<HTMLButtonElement>,
  'color'
> {
  color?: XperienceButtonColor;
  inProgress?: boolean;
  icon?: React.ReactNode;
}

export const Button = ({
  className,
  color = 'secondary',
  children,
  disabled,
  icon,
  inProgress,
  ...props
}: ButtonProps) => {
  const colorClass =
    color === 'tertiary'
      ? 'xp-buttonTertiary'
      : color === 'quinary'
        ? 'xp-buttonQuinary'
        : color === 'alert'
          ? 'xp-buttonAlert'
          : 'xp-buttonSecondary';

  return (
    <button
      className={cn('xp-button', colorClass, className)}
      disabled={disabled || inProgress}
      type="button"
      {...props}
    >
      {icon}
      {children}
    </button>
  );
};
