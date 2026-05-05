import { writable, derived, get } from 'svelte/store';
import en from './locales/en.json';

export type Locale = 'en' | 'hi';

const translations: Record<string, Record<string, string>> = { en };

export const locale = writable<Locale>('en');

// Lazy load non-English locales
let loadedLocales = new Set<string>(['en']);

export async function setLocale(newLocale: Locale) {
  if (!loadedLocales.has(newLocale)) {
    try {
      const mod = await import(`./locales/${newLocale}.json`);
      translations[newLocale] = mod.default;
      loadedLocales.add(newLocale);
    } catch {
      console.warn(`Failed to load locale: ${newLocale}, falling back to en`);
    }
  }
  locale.set(newLocale);
}

function interpolate(str: string, params: Record<string, string | number>): string {
  return str.replace(/\{(\w+)\}/g, (_, key) => {
    return params[key] !== undefined ? String(params[key]) : `{${key}}`;
  });
}

export const t = derived(locale, ($locale) => {
  return (key: string, params?: Record<string, string | number>): string => {
    const str = translations[$locale]?.[key] || translations['en']?.[key] || key;
    if (!params) return str;
    return interpolate(str, params);
  };
});

export function formatCurrency(amountPaise: number, loc?: Locale): string {
  const $locale = loc || get(locale);
  const amount = amountPaise / 100;
  return new Intl.NumberFormat($locale === 'hi' ? 'hi-IN' : 'en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0
  }).format(amount);
}

export function formatDate(date: Date | string, loc?: Locale): string {
  const $locale = loc || get(locale);
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat($locale === 'hi' ? 'hi-IN' : 'en-IN', {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  }).format(d);
}

export function formatTime(date: Date | string, loc?: Locale): string {
  const $locale = loc || get(locale);
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat($locale === 'hi' ? 'hi-IN' : 'en-IN', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true
  }).format(d);
}

export function formatRelativeTime(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffMin = Math.floor(diffMs / 60000);

  if (diffMin < 1) return 'Just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `${diffHr}h ago`;
  const diffDays = Math.floor(diffHr / 24);
  if (diffDays < 7) return `${diffDays}d ago`;
  return formatDate(d);
}
