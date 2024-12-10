import { transition, trigger, useAnimation } from '@angular/animations';
import { Component } from '@angular/core';
import { bounce, shakeX, tada } from 'ng-animate';
import { lastValueFrom, timer } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  animations: [
    // Animation 'shake' pour le carré rouge (secousses horizontales)
    trigger('shake', [
      transition(':increment', useAnimation(shakeX, { params: { timing: 2 } }))
    ]),
    
    // Animation 'bounce' pour le carré vert (rebond)
    trigger('bounce', [
      transition(':increment', useAnimation(bounce, { params: { timing: 4 } }))
    ]),
    
    // Animation 'tada' pour le carré bleu (effet de salut)
    trigger('tada', [
      transition(':increment', useAnimation(tada, { params: { timing: 3 } }))
    ]),
  ]
})
export class AppComponent {
  title = 'ngAnimations';

  // Variables de suivi des animations pour chaque carré
  ng_shake = 0;
  ng_bounce = 0;
  ng_tada = 0;
  css_rotate = false;  // Variable pour gérer la rotation CSS

  constructor() { }

  // Méthode pour déclencher la rotation CSS du carré orange
  rotatecss() {
    this.css_rotate = true;  // Active la rotation
    setTimeout(() => {
      // Après 2 secondes, désactive la rotation
      this.css_rotate = false;
    }, 2000);
  }

  // Méthode pour jouer la séquence d'animations : Shake, Bounce, Tada
  async bounceShakeFlip() {
    // Déclenche l'animation Shake (carré rouge)
    this.ng_shake++;
    await lastValueFrom(timer(2000)); // Attend la fin de l'animation Shake (2 secondes)

    // Déclenche l'animation Bounce (carré vert)
    this.ng_bounce++;
    await lastValueFrom(timer(3000)); // Attend la fin de l'animation Bounce (3 secondes)

    // Déclenche l'animation Tada (carré bleu)
    this.ng_tada++;
  }

  // Méthode pour jouer la boucle d'animations en continu
  async playLoop() {
    setTimeout(async () => {
      // Répète l'animation toutes les 8 secondes
      await this.playLoop();
    }, 8000);

    // Lance les animations dans l'ordre : Shake, Bounce, Tada
    await this.bounceShakeFlip();
  }
}

// import { transition, trigger, useAnimation } from '@angular/animations';
// import { Component } from '@angular/core';
// import { bounce, shakeX, tada } from 'ng-animate';
// import { lastValueFrom, timer } from 'rxjs';

// @Component({
//   selector: 'app-root',
//   templateUrl: './app.component.html',
//   styleUrls: ['./app.component.css'],

//   animations: [
//     trigger('shake', [transition(':increment', useAnimation(shakeX, { params: { timing: 2 } }))]),
//     trigger('bounce', [transition(':increment', useAnimation(bounce, { params: { timing: 4 } }))]),
//     trigger('tada', [transition(':increment', useAnimation(tada, { params: { timing: 3 } }))]),



//   ]
// })



// export class AppComponent {
//   title = 'ngAnimations';

//   ng_shake = 0;
//   ng_bounce = 0;
//   ng_tada = 0;
//   css_rotate = false;
//   constructor() { }

//   rotatecss()
//   { this.css_rotate = true 
//     setTimeout(() => {
//       // Après 3 secondes
//       this.css_rotate = false;
//     }, 2000);


//   }

//   async bounceShakeFlip() {
//     this.ng_shake++;
//     await lastValueFrom(timer(2000));
//     this.ng_bounce++;
//     await lastValueFrom(timer(3000));
//     this.ng_tada++;
//   }

//   async playLoop() {
//     setTimeout(async () => {
//       await this.playLoop();
//     }, 8000);
//     await this.bounceShakeFlip();
//   }
// }
