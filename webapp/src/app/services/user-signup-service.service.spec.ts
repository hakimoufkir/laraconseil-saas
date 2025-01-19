import { TestBed } from '@angular/core/testing';

import { UserSignupServiceService } from './user-signup-service.service';

describe('UserSignupServiceService', () => {
  let service: UserSignupServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserSignupServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
