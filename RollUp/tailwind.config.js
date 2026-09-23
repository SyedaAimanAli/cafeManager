/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.{razor,cshtml}',
    './Shared/**/*.razor',
    './Features/**/*.razor',
    './wwwroot/**/*.{html,js}',
  ],
  darkMode: ['class', '[data-theme="dark"]'],
  theme: {
    extend: {
      colors: {
        // Semantic accent — resolved at runtime via CSS custom property
        accent: {
          DEFAULT: 'var(--color-accent, #C4956A)',
          ink: 'var(--color-accent-ink, #ffffff)',
          muted: 'var(--color-accent-muted, rgba(196,149,106,0.15))',
        },
        // Semantic surface
        surface: 'var(--color-surface, #ffffff)',
        bg: 'var(--color-bg, #fafaf9)',
        ink: {
          DEFAULT: 'var(--color-ink, #1c1917)',
          muted: 'var(--color-ink-muted, #57534e)',
          faint: 'var(--color-ink-faint, #a8a29e)',
        },
        line: 'var(--color-line, #e7e5e4)',
        ok: {
          DEFAULT: 'var(--color-ok, #10b981)',
          bg: 'var(--color-ok-bg, #ecfdf5)',
          text: 'var(--color-ok-text, #065f46)',
        },
        warn: {
          DEFAULT: 'var(--color-warn, #f59e0b)',
          bg: 'var(--color-warn-bg, #fffbeb)',
          text: 'var(--color-warn-text, #92400e)',
        },
        bad: {
          DEFAULT: 'var(--color-bad, #ef4444)',
          bg: 'var(--color-bad-bg, #fef2f2)',
          text: 'var(--color-bad-text, #991b1b)',
        },
        muted: 'var(--color-ink-muted, #57534e)',
        // Legacy aliases — for untouched files (do NOT add new uses)
        cafe: {
          espresso: '#3D2314',
          mocha: '#6B4423',
          caramel: '#C4956A',
          latte: '#e8d5c4',
          cream: '#FDF6E9',
        },
      },
      fontFamily: {
        display: ['Bricolage Grotesque', 'Georgia', 'serif'],
        body: ['DM Sans', 'system-ui', 'sans-serif'],
        classic: ['Fraunces', 'Georgia', 'serif'],
        sans: ['DM Sans', 'system-ui', 'sans-serif'],
        // Legacy (untouched files)
        poppins: ['Poppins', 'sans-serif'],
        cinzel: ['Cinzel', 'serif'],
        cormorant: ['Cormorant Garamond', 'serif'],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'monospace'],
      },
      borderRadius: {
        card: '18px',
        pill: '9999px',
      },
      boxShadow: {
        card: '0 1px 3px 0 rgba(0,0,0,0.06), 0 1px 2px -1px rgba(0,0,0,0.04)',
        elevated: '0 4px 16px -2px rgba(0,0,0,0.10), 0 2px 4px -1px rgba(0,0,0,0.06)',
        modal: '0 20px 60px -10px rgba(0,0,0,0.18)',
        '2xs': '0 1px 2px 0 rgba(0,0,0,0.04)',
        'xs': '0 1px 3px 0 rgba(0,0,0,0.06)',
      },
    },
  },
  plugins: [],
};
