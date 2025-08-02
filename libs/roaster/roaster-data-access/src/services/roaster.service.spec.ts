import { TestBed } from '@angular/core/testing';

import { RoasterService } from './roaster.service';

describe('RoasterService', () => {
  let service: RoasterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RoasterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
