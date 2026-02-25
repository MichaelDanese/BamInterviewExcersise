import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PersonService } from './services/person.service';
import { AstronautService } from './services/astronaut.service';
import { AstronautDutyDTO, PersonAstronaut } from './models';

@Component({
  selector: 'app-root',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit{
  protected readonly title = signal('stargate-frontend');

  private readonly personApi = inject(PersonService);
  private readonly astronautApi = inject(AstronautService);
  private readonly formBuilder = inject(FormBuilder);

  readonly createPersonForm = this.formBuilder.nonNullable.group({
    personName: ['', [Validators.required]]
  });

  readonly createDutyForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required]],
    rank: ['', [Validators.required]],
    dutyTitle: ['', [Validators.required]],
    dutyStartDate: ['', [Validators.required]]
  });

  readonly searchForm = this.formBuilder.nonNullable.group({
    searchName: ['', [Validators.required]]
  });

  people = signal<PersonAstronaut[]>([]);
  selectedPerson = signal<PersonAstronaut | null>(null);
  selectedDuties = signal<AstronautDutyDTO[]>([]);

  isLoading = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  ngOnInit(): void {
    this.loadPeople();
  }

  clearMessages(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }

  loadPeople(): void {
    this.personApi.getPeople().subscribe({
      next: (res) => {
        this.people.set(res.people ?? []);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Failed to load people.');
      }
    });
  }

  onCreatePerson(): void {
    this.clearMessages();

    const name = this.createPersonForm.controls.personName.value.trim();
    if (!name) {
      this.errorMessage.set('Person name is required.');
      return;
    }

    this.isLoading.set(true);
    this.personApi.createPerson(name).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Person created.');
        this.createPersonForm.reset({ personName: '' });
        this.loadPeople();
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Failed to create person.');
        this.isLoading.set(false);
      },
      complete: () => this.isLoading.set(false)
    });
  }

  onCreateDuty(): void {
    this.clearMessages();

    const name = this.createDutyForm.controls.name.value.trim();
    const rank = this.createDutyForm.controls.rank.value.trim();
    const dutyTitle = this.createDutyForm.controls.dutyTitle.value.trim();
    const dutyStartDate = this.createDutyForm.controls.dutyStartDate.value;

    if (!name || !rank || !dutyTitle || !dutyStartDate) {
      this.errorMessage.set('All duty fields are required.');
      return;
    }

    this.isLoading.set(true);
    this.astronautApi.createAstronautDuty({
      name,
      rank,
      dutyTitle,
      dutyStartDate
    }).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Duty created.');
        this.createDutyForm.reset({ name: '', rank: '', dutyTitle: '', dutyStartDate: '' });
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Failed to create duty.');
        this.isLoading.set(false);
      },
      complete: () => this.isLoading.set(false)
    });
  }

  onSearchByName(): void {
    this.clearMessages();
    const name = this.searchForm.controls.searchName.value.trim();
    if (!name) {
      this.errorMessage.set('Search name is required.');
      return;
    }

    this.isLoading.set(true);
    this.astronautApi.getAstronautDutiesByName(name).subscribe({
      next: (res) => {
        this.selectedPerson.set(res.person);
        this.selectedDuties.set(res.astronautDuties ?? []);
        if (!res.person) this.errorMessage.set('No person found with that name.');
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Search failed.');
        this.isLoading.set(false);
      },
      complete: () => this.isLoading.set(false)
    });
  }
}
