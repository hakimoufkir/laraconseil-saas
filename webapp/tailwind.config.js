/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts}' // Include all HTML and TypeScript files in the src folder
  ],
  theme: {
    extend: {
      backgroundImage: {
        'custom-pattern': "url('/assets/images/underDev.png')",
      },
    },
  },
  plugins: [
    require('daisyui'),
  ],
};
