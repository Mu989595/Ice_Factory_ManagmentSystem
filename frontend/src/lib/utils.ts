import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-EG', {
    style: 'decimal',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount) + ' EGP';
};

export const getArabicRole = (role: string) => {
  const roles: Record<string, string> = {
    'WinchOperator': 'وناش',
    'IcePusher': 'بيزق التلج',
    'IceStacker': 'بيرص التلج',
  };
  return roles[role] || role;
};
