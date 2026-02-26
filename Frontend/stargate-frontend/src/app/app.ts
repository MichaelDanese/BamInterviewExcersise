import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { PersonService } from './services/person.service';
import { AstronautService } from './services/astronaut.service';
import { AstronautDutyDTO, PersonAstronaut } from './models';

@Component({
  selector: 'app-root',
  imports: [CommonModule, ReactiveFormsModule, NgxSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('stargate-frontend');
  readonly maxDutyStartDate = this.getTodayDate();

  private readonly personApi = inject(PersonService);
  private readonly astronautApi = inject(AstronautService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly spinner = inject(NgxSpinnerService);

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

  selectedPerson = signal<PersonAstronaut | null>(null);
  selectedDuties = signal<AstronautDutyDTO[]>([]);

  isLoading = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  private getTodayDate(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  clearMessages(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }

  private setLoading(loading: boolean): void {
    this.isLoading.set(loading);

    if (loading) {
      this.spinner.show('rocketSpinner');
      return;
    }

    this.spinner.hide('rocketSpinner');
  }

  onCreatePerson(): void {
    this.clearMessages();

    const name = this.createPersonForm.controls.personName.value.trim();
    if (!name) {
      this.errorMessage.set('Person name is required.');
      return;
    }

    this.setLoading(true);
    this.personApi.createPerson(name).pipe(
      finalize(() => this.setLoading(false))
    ).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Person created.');
        this.createPersonForm.reset({ personName: '' });
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Failed to create person.');
      }
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

    this.setLoading(true);
    this.astronautApi.createAstronautDuty({
      name,
      rank,
      dutyTitle,
      dutyStartDate
    }).pipe(
      finalize(() => this.setLoading(false))
    ).subscribe({
      next: (res) => {
        this.successMessage.set(res.message || 'Duty created.');
        this.selectedPerson.set(null);
        this.selectedDuties.set([]);
        this.createDutyForm.reset({ name: '', rank: '', dutyTitle: '', dutyStartDate: '' });
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Failed to create duty.');
      }
    });
  }

  onSearchByName(): void {
    this.clearMessages();
    const name = this.searchForm.controls.searchName.value.trim();
    if (!name) {
      this.errorMessage.set('Search name is required.');
      return;
    }

    this.setLoading(true);
    this.astronautApi.getAstronautDutiesByName(name).pipe(
      finalize(() => this.setLoading(false))
    ).subscribe({
      next: (res) => {
        this.selectedPerson.set(res.person);
        this.selectedDuties.set(res.astronautDuties ?? []);
        if (!res.person) this.errorMessage.set('No person found with that name.');
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Search failed.');
      }
    });
  }
}
