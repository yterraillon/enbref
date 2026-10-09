// EnBref — window.EnBref. Types as documentation.
import type { ReactNode, ButtonHTMLAttributes, InputHTMLAttributes } from "react";

export type IconName = "play" | "pause" | "skip-forward" | "check" | "chevron-right" | "chevron-up" | "chevron-down";

export interface IconProps { name: IconName; /** height in px; default = native size */ size?: number; }

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** primary: ink pill 50px. plain: ink text, no fill (« Activer », « Relire »). Default primary. */
  variant?: "primary" | "plain";
  icon?: IconName | ReactNode;
  /** full width (main card action) */
  block?: boolean;
  children?: ReactNode;
}

export interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** surface: white disc with shadow (« Aa »). filled: ink disc (play/pause). ghost: no fill. Default surface. */
  variant?: "surface" | "filled" | "ghost";
  icon?: IconName | ReactNode;
  /** accessible name — required */
  label: string;
  children?: ReactNode;
}

export interface SwitchProps { checked?: boolean; defaultChecked?: boolean; onChange?: (v: boolean) => void; label?: string; disabled?: boolean; }

export interface SegmentedControlProps { options: string[]; value?: string; defaultValue?: string; onChange?: (v: string) => void; label?: string; }

export interface CheckCircleProps { checked?: boolean; defaultChecked?: boolean; onChange?: (v: boolean) => void; children?: ReactNode; }

export interface ChoiceChipsProps { options: string[]; value?: string; defaultValue?: string; onChange?: (v: string) => void; columns?: number; label?: string; }

export interface ListSectionProps { header?: ReactNode; footer?: ReactNode; children: ReactNode; }

export interface ListRowProps {
  title: ReactNode;
  /** second line; makes the row 60px */
  subtitle?: ReactNode;
  /** value pill (e.g. "7:30") */
  value?: ReactNode;
  /** ink text action at the end (« Activer ») */
  action?: ReactNode;
  /** any trailing control, typically <Switch/> */
  trailing?: ReactNode;
  chevron?: boolean;
  /** 48px row */
  compact?: boolean;
  /** hidden item: title at 40% */
  dimmed?: boolean;
  onMoveUp?: () => void;
  onMoveDown?: () => void;
  onClick?: () => void;
}

export interface LargeTitleProps { eyebrow?: ReactNode; title: ReactNode; trailing?: ReactNode; }

export interface ProgressSegmentsProps { segments: Array<"done" | "current" | "todo">; label?: string; }

export interface SummaryCardProps {
  read: number; total: number; minutes?: number;
  segments: Array<"done" | "current" | "todo">;
  playing?: boolean; playLabel?: string; onPlay?: () => void;
}

export interface Brief { title: string; body: string; }
export interface RubricCardProps {
  name: string; items: Brief[];
  read?: boolean; defaultRead?: boolean; onReadChange?: (v: boolean) => void;
  /** being read aloud */
  current?: boolean;
  /** Dynamic Type step, default "m" */
  textSize?: "s" | "m" | "l";
}

export interface MiniPlayerProps { title: ReactNode; label?: ReactNode; rate?: string; playing?: boolean; onRate?: () => void; onNext?: () => void; onToggle?: () => void; }

export interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> { label?: string; hint?: string; error?: string; }

export interface StatusBadgeProps { tone?: "neutral" | "success" | "warning" | "danger"; children: ReactNode; }
