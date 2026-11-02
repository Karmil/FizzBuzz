import { ScrollingModule } from '@angular/cdk/scrolling';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const ROW_HEIGHT_PX = 36;
const MAX_VIEWPORT_HEIGHT_PX = 432;

/**
 * Displays the sequence with virtual scrolling: only the visible rows are in the DOM,
 * so 10 000 results render as fast as 15.
 */
@Component({
  selector: 'app-fizzbuzz-results',
  imports: [ScrollingModule],
  templateUrl: './fizzbuzz-results.html',
  styleUrl: './fizzbuzz-results.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FizzBuzzResults {
  readonly values = input.required<readonly string[]>();

  protected readonly rowHeight = ROW_HEIGHT_PX;
  protected readonly viewportHeight = computed(
    () => Math.min(this.values().length * ROW_HEIGHT_PX, MAX_VIEWPORT_HEIGHT_PX) + 2,
  );
  protected readonly replacedCount = computed(
    () => this.values().filter((value, index) => !isPlainNumber(value, index)).length,
  );

  protected isReplaced(value: string, index: number): boolean {
    return !isPlainNumber(value, index);
  }
}

function isPlainNumber(value: string, index: number): boolean {
  return value === String(index + 1);
}
