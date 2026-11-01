/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "../Views/**/*.cshtml",
    "../Areas/**/*.cshtml",
    "./js/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: 'rgb(139, 1, 1)',
          hover: 'rgb(119, 1, 1)',
          light: 'rgb(211, 77, 77)',
          lighter: 'rgb(255, 235, 235)',
        },
        secondary: '#20222A',
        dark: '#1a1c23',
      },
      borderRadius: {
        'sm': '4px',
        'md': '8px',
        'lg': '12px',
        'xl': '16px',
        '2xl': '20px',
        'full': '9999px',
      },
      boxShadow: {
        'sm': '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
        'DEFAULT': '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px -1px rgba(0, 0, 0, 0.1)',
        'md': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)',
        'lg': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1)',
        'xl': '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)',
        '2xl': '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
        'inner': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.05)',
        'primary': '0 4px 14px 0 rgba(139, 1, 1, 0.25)',
        'primary-lg': '0 10px 25px -5px rgba(139, 1, 1, 0.3)',
      },
      transitionDuration: {
        '150': '150ms',
        '200': '200ms',
        '300': '300ms',
      },
      transitionTimingFunction: {
        'ease-out': 'cubic-bezier(0, 0, 0.2, 1)',
        'ease-in-out': 'cubic-bezier(0.4, 0, 0.2, 1)',
      },
      fontFamily: {
        sans: ['system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'sans-serif'],
      },
      fontSize: {
        'xs': ['0.75rem', { lineHeight: '1rem' }],
        'sm': ['0.875rem', { lineHeight: '1.25rem' }],
        'base': ['1rem', { lineHeight: '1.5rem' }],
        'lg': ['1.125rem', { lineHeight: '1.75rem' }],
        'xl': ['1.25rem', { lineHeight: '1.75rem' }],
        '2xl': ['1.5rem', { lineHeight: '2rem' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
      },
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem',
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'scale-in': 'scaleIn 0.2s ease-out',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        scaleIn: {
          '0%': { transform: 'scale(0.95)', opacity: '0' },
          '100%': { transform: 'scale(1)', opacity: '1' },
        },
      },
    },
  },
  plugins: [
    function({ addComponents, theme }) {
      addComponents({
        '.btn-primary': {
          backgroundColor: theme('colors.primary.DEFAULT'),
          color: '#fff',
          padding: '0.5rem 1.25rem',
          borderRadius: theme('borderRadius.md'),
          fontWeight: '500',
          transition: 'all 0.2s ease-in-out',
          boxShadow: theme('boxShadow.sm'),
          '&:hover': {
            backgroundColor: theme('colors.primary.hover'),
            boxShadow: theme('boxShadow.primary'),
            transform: 'translateY(-1px)',
          },
          '&:active': {
            transform: 'translateY(0)',
            boxShadow: theme('boxShadow.sm'),
          },
        },
        '.btn-secondary': {
          backgroundColor: '#fff',
          color: theme('colors.primary.DEFAULT'),
          padding: '0.5rem 1.25rem',
          borderRadius: theme('borderRadius.md'),
          fontWeight: '500',
          border: '1px solid ' + theme('colors.primary.DEFAULT'),
          transition: 'all 0.2s ease-in-out',
          '&:hover': {
            backgroundColor: theme('colors.primary.lighter'),
            boxShadow: theme('boxShadow.sm'),
          },
        },
        '.card': {
          backgroundColor: '#fff',
          borderRadius: theme('borderRadius.lg'),
          boxShadow: theme('boxShadow.DEFAULT'),
          transition: 'all 0.3s ease-in-out',
          '&:hover': {
            boxShadow: theme('boxShadow.lg'),
          },
        },
        '.input-field': {
          width: '100%',
          padding: '0.625rem 0.875rem',
          borderRadius: theme('borderRadius.md'),
          border: '1px solid #e5e7eb',
          transition: 'all 0.15s ease-in-out',
          '&:focus': {
            outline: 'none',
            borderColor: theme('colors.primary.DEFAULT'),
            boxShadow: '0 0 0 3px rgba(139, 1, 1, 0.1)',
          },
        },
      });
    },
  ],
};
