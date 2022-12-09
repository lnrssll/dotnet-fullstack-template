import React from 'react';
import logo from './logo.svg';
import style9 from "style9";

function App() {
  return (
    <div className={styles('App')}>
      <header className={styles('AppHeader')}>
        <img src={logo} className={styles('AppLogo')} alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className={styles('AppLink')}
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
  );
}

const styles = style9.create({
  App: {
      textAlign: 'center'
  },
  AppLogo: {
    height: '40vmin',
    pointerEvents: 'none',
    '@media (prefers-reduced-motion: no-preference)': {
      animationName: style9.keyframes({
        from: { transform: 'rotate(0deg)' },
        to: { transform: 'rotate(360deg)' }
      }),
      animationIterationCount: 'infinite',
      animationDuration: '20s',
      animationTimingFunction: 'linear'
    }
  },
  AppHeader: {
    backgroundColor: '#282c34',
    minHeight: '100vh',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    fontSize: 'calc(10px + 2vmin)',
    color: 'white'
  },
  AppLink: {
    color: '#61dafb'
  }
});

export default App;
