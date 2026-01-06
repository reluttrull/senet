import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameInfoBlock } from './game-info-block';

describe('GameInfoBlock', () => {
  let component: GameInfoBlock;
  let fixture: ComponentFixture<GameInfoBlock>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameInfoBlock]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameInfoBlock);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
