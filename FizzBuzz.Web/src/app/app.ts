import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FizzBuzzPage } from './features/fizzbuzz/fizzbuzz-page/fizzbuzz-page';

@Component({
  selector: 'app-root',
  imports: [FizzBuzzPage],
  template: `
    <header class="app-header">
      <div class="container">
        <h1>FizzBuzz</h1>
        <p>Générez une séquence FizzBuzz personnalisée, calculée par l'API.</p>
      </div>
    </header>
    <main class="container" id="main">
      <app-fizzbuzz-page />
    </main>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
